using ApiFramework.Exceptions;

namespace ResoniteApi.Exceptions
{
    internal class ApiInvalidCloudVariablePathException : ApiException
    {
        private string _path;

        public ApiInvalidCloudVariablePathException(string path) : base(400)
        {
            _path = path;
        }

        public override string ToString()
        {
            return $"Invalid Cloud Variable Path: '{_path}'";
        }
    }
}
