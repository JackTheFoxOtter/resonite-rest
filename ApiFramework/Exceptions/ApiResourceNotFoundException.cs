using System;

namespace ApiFramework.Exceptions
{
    public class ApiResourceNotFoundException : ApiException
    {
        private readonly Type _resourceType;
        private readonly string _resourceId;

        public string ResourceId => _resourceId;
        public Type ResourceType => _resourceType;

        public ApiResourceNotFoundException(Type resourceType, string resourceId) : base(404)
        {
            _resourceType = resourceType;
            _resourceId = resourceId;
        }

        public override string ToString()
        {
            return $"No {_resourceType.Name} found for Id '{_resourceId}'.";
        }
    }
}
