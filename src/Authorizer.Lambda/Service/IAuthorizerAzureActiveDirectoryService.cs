using Amazon.Lambda.APIGatewayEvents;

namespace Authorizer.Lambda.Service
{
    public interface IAuthorizerAzureActiveDirectoryService
    {
        APIGatewayCustomAuthorizerResponse GenerateAuthorization(string token, string methodArn);
    }
}
