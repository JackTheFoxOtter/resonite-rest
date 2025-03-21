using ApiFramework.Exceptions;

namespace ResoniteApi.Exceptions
{
    internal class ApiCloudVariableWriteException : ApiException
    {
        public ApiCloudVariableWriteException() : base(400) { }

        public override string ToString()
        {
            return "Failed to write to cloud variable.";
        }
    }
}
