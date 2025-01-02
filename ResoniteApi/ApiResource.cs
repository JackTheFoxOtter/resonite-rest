using System.Collections.Generic;
using System.Linq;

namespace ResoniteApi
{
    internal abstract class ApiResource
    {
        private readonly Dictionary<string, ApiItem> _items;

        public ApiResource()
        {
            _items = new Dictionary<string, ApiItem>();
        }

        protected void AddItem(string name, object item)
        {
            AddItem(name, item, false);
        }

        protected void AddItem(string name, object item, bool readOnly)
        {
            _items.Add(name, new ApiItem(name, item, readOnly));
        }

        public bool ContainsItem(string name)
        {
            return _items.ContainsKey(name);
        }

        public ApiItem this[string name]
        {
            get
            {
                return _items[name];
            }
        }

        public Dictionary<string, object> GetJsonRepresentation()
        {
            return _items.ToDictionary(kv => kv.Key, kv => kv.Value.Value);
        }
    }
}
