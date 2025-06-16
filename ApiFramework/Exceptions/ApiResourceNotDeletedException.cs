using System;

namespace ApiFramework.Exceptions
{
    public class ApiResourceNotDeletedException : ApiException
    {
        private readonly Type _resourceType;
        private readonly string _resourceId;

        public string ResourceId => _resourceId;
        public Type ResourceType => _resourceType;

        public ApiResourceNotDeletedException(Type resourceType, string resourceId) : base(500)
        {
            _resourceType = resourceType;
            _resourceId = resourceId;
        }

        public override string ToString()
        {
            return $"Failed to delete {_resourceType.Name} resource with Id '{_resourceId}'.";
        }
    }
}
