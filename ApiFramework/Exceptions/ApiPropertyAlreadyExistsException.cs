using ApiFramework.Resources;

namespace ApiFramework.Exceptions
{
    internal class ApiPropertyAlreadyExistsException : ApiException
    {
        public ApiPropertyInfo PropertyInfo { get; }

        public ApiPropertyAlreadyExistsException(ApiPropertyInfo propertyInfo) : base(400) 
        {
            PropertyInfo = propertyInfo;
        }

        public override string ToString()
        {
            return $"Property {PropertyInfo.Path} already exists!";
        }
    }
}
