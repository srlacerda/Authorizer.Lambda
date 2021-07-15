using System;
using System.Collections.Generic;
using System.Text;

namespace Authorizer.Lambda.Mediator.Base
{
    public interface IResult
    {
        int ResponseCode { get; set; }
        object Content { get; set; }
        bool Sucess { get; set; }
        Exception Exception { get; set; }
    }
}
