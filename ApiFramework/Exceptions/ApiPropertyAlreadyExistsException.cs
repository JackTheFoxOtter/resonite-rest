using ApiFramework.Resources;
using ApiFramework.Resources.Properties;

namespace ApiFramework.Exceptions
{
    internal class ApiPropertyAlreadyExistsException : ApiException
    {
        public ApiProperty Property { get; }

        public ApiPropertyAlreadyExistsException(ApiProperty property) : base(400) 
        {
            Property = property;
        }

        public override string ToString()
        {
            return $"Property {Property.Path} already exists!";
        }
    }
}
