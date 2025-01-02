using ResoniteApi.Exceptions;

namespace ResoniteApi
{
    internal class ApiItem
    {
        private readonly string _itemName;
        private readonly bool _itemReadOnly;
        private object _itemValue;

        public ApiItem(string name, object value, bool readOnly)
        {
            _itemName = name;
            _itemValue = value;
            _itemReadOnly = readOnly;
        }

        public string Name => _itemName;
        public bool ReadOnly => _itemReadOnly;
        public object Value
        {
            get 
            { 
                return _itemValue; 
            }
            set
            {
                if (_itemReadOnly) throw new ApiValueReadOnlyException(_itemName);
                _itemValue = value;
            }
        }
    }
}
