namespace ApiFramework.Exceptions
{

    internal class ApiMethodNotSupportedException : ApiException
    {
        private string _endpointName;
        private string _methodName;

        public ApiMethodNotSupportedException(string endpointName, string methodName) : base(405)
        {
            _endpointName = endpointName;
            _methodName = methodName;
        }

        public override string ToString()
        {
            return $"Method '{_methodName}' not supported for endpoint '{_endpointName}'.";
        }
    }
}
