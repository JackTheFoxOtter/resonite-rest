using System;

namespace ApiFramework.Exceptions
{
    public class ApiInvalidItemTypeException : ApiException
    {
        public Type ResourceType { get; }
        public Type Type { get; }

        public ApiInvalidItemTypeException(Type resourceType, Type type) : base(500)
        {
            ResourceType = resourceType;
            Type = type;
        }

        public override string ToString()
        {
            return $"Type {Type.Name} of resource {ResourceType.Name} is not a valid API item type!";
        }
    }
}
