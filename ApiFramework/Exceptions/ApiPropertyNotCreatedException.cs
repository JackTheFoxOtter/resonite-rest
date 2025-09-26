using ApiFramework.Resources;
using ApiFramework.Resources.Properties;

namespace ApiFramework.Exceptions
{
    internal class ApiPropertyNotCreatedException : ApiException
    {
        ApiProperty Path { get; }
        
        public ApiPropertyNotCreatedException(ApiProperty path) : base(400)
        {
            Path = path;
        }

        public override string ToString()
        {
            return $"Failed to create property {Path}!";
        }
    }
}
