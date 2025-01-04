using ApiFramework.Exceptions;
using Newtonsoft.Json;
using System;

namespace ApiFramework.Resources
{
    public class ApiItem
    {
        private readonly string _itemName;
        private readonly bool _itemReadOnly;
        private readonly Type _itemType;
        private object? _itemValue;

        public ApiItem(string name, object? value, bool readOnly) : this(name, value, readOnly, value.GetType()) { }
        public ApiItem(string name, object? value, bool readOnly, Type itemType)
        {
            _itemName = name;
            _itemValue = value;
            _itemReadOnly = readOnly;
            _itemType = itemType;
        }

        public string Name => _itemName;
        public bool ReadOnly => _itemReadOnly;
        public Type ItemType => _itemType;
        public object? Value
        {
            get
            {
                return _itemValue;
            }
            set
            {
                if (_itemReadOnly) throw new ApiResourceItemReadOnlyException(_itemName);
                _itemValue = value;
            }
        }

        public ApiResponse ToResponse()
        {
            return new ApiResponse(200, JsonConvert.SerializeObject(_itemValue));
        }
    }
}
