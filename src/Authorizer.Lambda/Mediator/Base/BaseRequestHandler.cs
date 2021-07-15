using Authorizer.Lambda.Logger;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Authorizer.Lambda.Mediator.Base
{
    public abstract class BaseRequestHandler<TRequest> : IRequestHandler<TRequest, Result> where TRequest : IRequest<Result>
    {
        protected ILogger Logger;
        public BaseRequestHandler(ILogger logger)
        {
            Logger = logger;
        }

        protected abstract bool RequiresValidation();

        internal abstract string ValidateRequest(TRequest request);

        internal abstract Result Execute(TRequest request, CancellationToken cancellationToken);
        public Task<Result> Handle(TRequest request, CancellationToken cancellationToken)
        {
            Result result = null;

            try
            {
                if (RequiresValidation())
                {
                    var validationMessage = ValidateRequest(request);

                    if (!string.IsNullOrEmpty(validationMessage))
                    {
                        result = new Result
                        {
                            Sucess = false,
                            Exception = new Exception(validationMessage)
                        };

                        return Task.FromResult(result);
                    }
                }

                result = Execute(request, cancellationToken);

                if (result == null)
                {
                    result = new Result() { Sucess = false, Exception = new Exception("The respective command execution returned a null response.") };
                }
                else
                {
                    result.Sucess = true;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "An error has ocurred during the request.");
                throw;
            }

            return Task.FromResult(result);
        }
    }
}
