using Amazon.Lambda.APIGatewayEvents;
using Authorizer.Lambda.Logger;
using Authorizer.Lambda.Mediator.Base;
using Authorizer.Lambda.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Authorizer.Lambda.Mediator.Commands
{
    public class AuthorizerCommandHandler : BaseRequestHandler<AuthorizerCommand>
    {
        IAuthorizerAzureActiveDirectoryService _authorizerAzureActiveDirectoryService;
        public AuthorizerCommandHandler(IAuthorizerAzureActiveDirectoryService authorizerAzureActiveDirectoryService, ILogger logger) : base(logger)
        {
            _authorizerAzureActiveDirectoryService = authorizerAzureActiveDirectoryService;
        }
        protected override bool RequiresValidation()
        {
            return false;
        }

        internal override Result Execute(AuthorizerCommand request, CancellationToken cancellationToken)
        {
            Logger.Info("AuthorizeCommand started...");

            Logger.Info($"AuthorizeCommand.Message: {request.Message}");

            var authorizerRequest = JsonConvert.DeserializeObject<APIGatewayCustomAuthorizerRequest>(request.Message);

            Logger.Info("Authorizing...");

            var authorizerResponse = _authorizerAzureActiveDirectoryService.GenerateAuthorization(authorizerRequest.AuthorizationToken, authorizerRequest.MethodArn);

            if (authorizerResponse == null)
            {
                Logger.Info("Unable to authorizing.");
                return null;
            }

            return new Result()
            {
                Content = JsonConvert.SerializeObject(
                    authorizerResponse,
                    Formatting.None
                    ),
                ResponseCode = 200
            };
        }

        internal override string ValidateRequest(AuthorizerCommand request)
        {
            return "";
        }
    }
}
