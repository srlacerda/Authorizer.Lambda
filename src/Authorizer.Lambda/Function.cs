using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Authorizer.Lambda.Extensions;
using Authorizer.Lambda.Logger;
using Authorizer.Lambda.Mediator.Commands;
using Authorizer.Lambda.Service;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Authorizer.Lambda
{
    public class Functions
    {
        protected IServiceProvider _serviceProvider = null;
        protected ServiceCollection _serviceCollection = new ServiceCollection();
        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions()
        {
            ConfigureServices();
        }

        private void ConfigureServices()
        {
            IConfigurationRoot configuration = GetConfiguration();
            _serviceCollection.AddSingleton<IConfiguration>(configuration);


            //Services
            _serviceCollection.AddTransient<IAuthorizerAzureActiveDirectoryService, AuthorizerAzureActiveDirectoryService>();

            //Internal
            _serviceCollection.AddSingleton<ILogger, Authorizer.Lambda.Logger.Logger>();
            _serviceCollection.AddMediatorHandlers(typeof(Functions).Assembly);
            _serviceCollection.AddScoped<IMediator, MediatR.Mediator>();
            _serviceCollection.AddScoped<ServiceFactory>(sp => sp.GetService);

            _serviceProvider = _serviceCollection.BuildServiceProvider();
        }

        private static IConfigurationRoot GetConfiguration()
        {
#if DEBUG
            var env = "dev";
#else
            var env = Environment.GetEnvironmentVariable("env");
#endif
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile($"appsettings.json", false, true)
                            .AddJsonFile($"appsettings.{env}.json", false)
                            .AddEnvironmentVariables();

            var configuration = builder.Build();

            return configuration;
        }

        public APIGatewayCustomAuthorizerResponse Authorize(APIGatewayCustomAuthorizerRequest request, ILambdaContext context)
        {
            var command = new AuthorizerCommand(JsonConvert.SerializeObject(request));
            var mediator = _serviceProvider.GetService<IMediator>();

            var result = mediator.Send(command).GetAwaiter().GetResult();

            return JsonConvert.DeserializeObject<APIGatewayCustomAuthorizerResponse>(result.Content.ToString());
        }
    }
}
