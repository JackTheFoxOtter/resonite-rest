using ApiFramework.Resources;
using System;

namespace ApiFramework.Exceptions
{
    public class ApiInvalidPropertyException : ApiException
    {
        public Type ResourceType { get; }
        public ApiPropertyPath Path { get; }

        public ApiInvalidPropertyException(Type resourceType, ApiPropertyPath path)
        {
            ResourceType = resourceType;
            Path = path;
        }

        public override string ToString()
        {
            return $"Resource {ResourceType.Name} doesn't define property at path {Path}!";
        }
    }
}
