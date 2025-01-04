using System;

namespace ApiFramework.Exceptions
{
    public class ApiResourceItemNotFoundException : ApiException
    {
        private readonly Type _resourceType;
        private readonly string _itemName;

        public string ItemName => _itemName;

        public ApiResourceItemNotFoundException(Type resourceType, string argumentName) : base(400)
        {
            _resourceType = resourceType;
            _itemName = argumentName;
        }

        public override string ToString()
        {
            return $"{_resourceType.Name} doesn't contain item with name '{_itemName}'.";
        }
    }
}
