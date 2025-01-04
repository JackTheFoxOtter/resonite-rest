using Newtonsoft.Json;
using System;
using System.Net;

namespace ApiFramework.Exceptions
{
    public abstract class ApiException : Exception
    {
        private readonly int _statusCode;

        public int StatusCode => _statusCode;

        public ApiException() : this(500) { }
        public ApiException(int statusCode) : base()
        {
            _statusCode = statusCode;
        }

        public override string ToString()
        {
            return "Something went wrong!";
        }

        public ApiResponse ToResponse()
        {
            return new ApiResponse(_statusCode, JsonConvert.SerializeObject(ToString()));
        }
    }
}
