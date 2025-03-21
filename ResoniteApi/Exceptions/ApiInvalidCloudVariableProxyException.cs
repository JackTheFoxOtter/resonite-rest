using ApiFramework.Exceptions;

namespace ResoniteApi.Exceptions
{
    internal class ApiInvalidCloudVariableProxyException : ApiException
    {
        public ApiInvalidCloudVariableProxyException() : base(500) { }

        public override string ToString()
        {
            return "Cloud variable proxy has invalid state.";
        }
    }
}
