namespace ApiFramework.Exceptions
{

    internal class ApiMethodNotmplementedException : ApiException
    {
        private string _endpointName;
        private string _methodName;

        public ApiMethodNotmplementedException(string endpointName, string methodName) : base(405)
        {
            _endpointName = endpointName;
            _methodName = methodName;
        }

        public override string ToString()
        {
            return $"Method '{_methodName}' not implemented for endpoint '{_endpointName}'.";
        }
    }
}
