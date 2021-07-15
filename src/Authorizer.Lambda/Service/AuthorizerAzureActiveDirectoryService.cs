using Amazon.Lambda.APIGatewayEvents;
using Authorizer.Lambda.Logger;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorizer.Lambda.Service
{
    public class AuthorizerAzureActiveDirectoryService : IAuthorizerAzureActiveDirectoryService
    {
        private ILogger _logger;
        private readonly IConfiguration _configuration;
        private string _tenantId;
        private string _stsDiscoveryEndpoint;
        private List<string> _issuers;
        public AuthorizerAzureActiveDirectoryService(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public APIGatewayCustomAuthorizerResponse GenerateAuthorization(string token, string methodArn)
        {
            bool isAuthorized = Authorize(token, methodArn);
            return GenerateAuthorizationResponse(isAuthorized, methodArn);
        }

        private void LoadConfiguration()
        {
            _tenantId = _configuration["AzureAD:TenantId"];
            _stsDiscoveryEndpoint = _configuration["AzureAD:StsDiscoveryEndpoint"].Replace("#TENANTID#", _tenantId);
            _issuers = GetIssuers();
        }

        private List<string> GetIssuers()
        {
            byte count = 0;
            var lstIssuers = new List<string>();
            while (_configuration[$"azuread:issuers:{count}"] != null)
            {
                lstIssuers.Add(_configuration[$"azuread:issuers:{count}"].Replace("#TENANTID#", _tenantId));
                count++;
            }

            return lstIssuers;
        }

        private async Task<OpenIdConnectConfiguration> GetIssuerSigninKeys()
        {
            ConfigurationManager<OpenIdConnectConfiguration> configManager = new
                ConfigurationManager<OpenIdConnectConfiguration>(_stsDiscoveryEndpoint, new OpenIdConnectConfigurationRetriever());

            try
            {
                return await configManager.GetConfigurationAsync().ConfigureAwait(true);
            }
            catch (Exception)
            {
                _logger.Error($"Error in getting Configuration at Azure Active Directory: {_stsDiscoveryEndpoint}");
                throw;
            }
        }

        private bool Authorize(string token, string methodArn)
        {
            bool isAuthenticated = false;
            bool isAuthorized = false;
            string jwt, methodArnLast, applicationId;

            LoadConfiguration();

            var config = GetIssuerSigninKeys().GetAwaiter().GetResult();

            methodArnLast = methodArn.Split("/").Last();
            applicationId = _configuration[$"ApplicationId:{methodArnLast}"];
            jwt = token.Remove(0, 7);

            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = _issuers, //EmissorDoToken
                ValidateAudience = true,
                ValidAudience = applicationId, //ServicoConsumido
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = config.SigningKeys,
                SaveSigninToken = true,
                ValidateLifetime = true
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var result = tokenHandler.ValidateToken(jwt, validationParameters, out _);
                isAuthenticated = result.Identity.IsAuthenticated;
                isAuthorized = result.Claims.Any(c => c.Value.Equals(methodArnLast));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error ocurred validating token.");
            }

            if (isAuthenticated && isAuthorized)
                _logger.Info("Successfully authorized.");

            if (!isAuthorized)
                _logger.Info($"Token does not contain the {methodArnLast} claims/role");

            return isAuthenticated && isAuthorized;
        }

        private APIGatewayCustomAuthorizerResponse GenerateAuthorizationResponse(bool isAuthorized, string methodArn)
        {
            APIGatewayCustomAuthorizerPolicy policy = new APIGatewayCustomAuthorizerPolicy
            {
                Version = "2012-10-17",
                Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>()
            };

            policy.Statement.Add(new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
            {
                Action = new HashSet<string>(new string[] { "execute-api:Invoke" }),
                Effect = isAuthorized ? "Allow" : "Deny",
                Resource = new HashSet<string>(new string[] {methodArn})
            });

            return new APIGatewayCustomAuthorizerResponse
            {
                PolicyDocument = policy
            };
        }
    }
}
