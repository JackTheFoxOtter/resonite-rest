using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFramework.Exceptions
{
    public class ApiInvalidRequestBodyException : ApiException
    {
        public ApiInvalidRequestBodyException() : base(400) { }

        public override string ToString()
        {
            return "The request body is invalid for this endpoint.";
        }
    }
}
