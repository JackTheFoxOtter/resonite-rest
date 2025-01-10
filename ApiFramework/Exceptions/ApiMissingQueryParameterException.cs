namespace ApiFramework.Exceptions
{
    public class ApiMissingQueryParameterException : ApiException
    {
        private string _paramName;
        public string ParamName => _paramName;

        public ApiMissingQueryParameterException(string paramName) : base(400)
        {
            _paramName = paramName;
        }

        public override string ToString()
        {
            return $"Request is missing required query parameter '{_paramName}'";
        }
    }
}
