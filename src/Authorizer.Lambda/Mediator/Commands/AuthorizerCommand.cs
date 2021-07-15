using Authorizer.Lambda.Mediator.Base;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Authorizer.Lambda.Mediator.Commands
{
    public class AuthorizerCommand : IRequest<Result>
    {
        public string Message { get; set; }
        public AuthorizerCommand()
        {

        }
        public AuthorizerCommand(string message)
        {
            Message = message;
        }
    }
}
