using ApiFramework.Resources;

namespace ApiFramework.Exceptions
{
    internal class ApiPropertyNotCreatedException : ApiException
    {
        ApiPropertyPath Path { get; }
        
        public ApiPropertyNotCreatedException(ApiPropertyPath path) : base(400) 
        {
            Path = path;
        }

        public override string ToString()
        {
            return $"Failed to create property {Path}!";
        }
    }
}
