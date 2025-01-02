namespace ResoniteApi.Exceptions
{
    internal class ApiValueReadOnlyException : ApiException
    {
        private readonly string _valueName;

        public ApiValueReadOnlyException(string valueName)
        {
            _valueName = valueName;
        }

        public string ValueName => _valueName;
    }
}
