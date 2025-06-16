using System;

namespace ApiFramework.Exceptions
{
    internal class ApiResourceNotUpdatedException : ApiException
    {
        private readonly Type _resourceType;
        private readonly string _resourceId;

        public string ResourceId => _resourceId;
        public Type ResourceType => _resourceType;

        public ApiResourceNotUpdatedException(Type resourceType, string resourceId) : base(500)
        {
            _resourceType = resourceType;
            _resourceId = resourceId;
        }

        public override string ToString()
        {
            return $"Failed to update {_resourceType.Name} resource with Id '{_resourceId}'.";
        }
    }
}
