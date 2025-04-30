using System;

namespace ApiFramework.Exceptions
{
    public class ApiResourceNotCreatedException : ApiException
    {
        private readonly Type _resourceType;

        public Type ResourceType => _resourceType;

        public ApiResourceNotCreatedException(Type resourceType) : base(404)
        {
            _resourceType = resourceType;
        }

        public override string ToString()
        {
            return $"Failed to create {_resourceType.Name} resource.";
        }
    }
}
