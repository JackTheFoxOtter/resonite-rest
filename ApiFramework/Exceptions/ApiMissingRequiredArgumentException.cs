using System;

namespace ApiFramework.Exceptions
{
    public class ApiMissingRequiredArgumentException : Exception
    {
        private readonly string _argumentName;

        public string ArgumentName => _argumentName;

        public ApiMissingRequiredArgumentException(string argumentName)
        {
            _argumentName = argumentName;
        }

        public override string ToString()
        {
            return $"Missing required argument: '{_argumentName}'";
        }
    }
}
