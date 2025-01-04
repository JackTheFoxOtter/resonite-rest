using Newtonsoft.Json;

namespace ApiFramework.Exceptions
{
    public class ApiResourceItemReadOnlyException : ApiException
    {
        private readonly string _valueName;

        public string ValueName => _valueName;

        public ApiResourceItemReadOnlyException(string valueName) : base(409)
        {
            _valueName = valueName;
        }

        public override string ToString()
        {
            return $"The ApiValue '{_valueName}' is read-only and can't be changed.";
        }

    }
}
