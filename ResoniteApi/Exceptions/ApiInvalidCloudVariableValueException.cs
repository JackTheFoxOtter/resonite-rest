using ApiFramework.Exceptions;

namespace ResoniteApi.Exceptions
{
    internal class ApiInvalidCloudVariableValueException : ApiException
    {
        public string _variableType;
        public string _value;

        public ApiInvalidCloudVariableValueException(string variableType, string value) : base(400)
        {
            _value = value;
            _variableType = variableType;
        }

        public override string ToString()
        {
            return $"'{_value}' is not a valid value for cloud variable of type {_variableType}!";
        }
    }
}
