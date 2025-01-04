using ApiFramework.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiFramework.Resources
{
    public abstract class ApiResource
    {
        private readonly Dictionary<string, ApiItem> _items;

        public ApiResource()
        {
            _items = new Dictionary<string, ApiItem>();
        }

        protected void AddItem(string name, object? item)
        {
            _items.Add(name, new ApiItem(name, item, false));
        }

        protected void AddItem(string name, object? item, bool readOnly)
        {
            _items.Add(name, new ApiItem(name, item, readOnly));
        }

        protected void AddItem(string name, object? item, Type type)
        {
            _items.Add(name, new ApiItem(name, item, false, type));
        }

        protected void AddItem(string name, object? item, bool readOnly, Type type)
        {
            _items.Add(name, new ApiItem(name, item, readOnly, type));
        }

        public bool ContainsItem(string name)
        {
            return _items.ContainsKey(name);
        }

        public ApiItem? this[string name]
        {
            get
            {
                if (ContainsItem(name))
                {
                    return _items[name];
                }

                return null;
            }
        }

        public Dictionary<string, object?> GetJsonRepresentation()
        {
            return _items.ToDictionary(kv => kv.Key, kv => kv.Value.Value);
        }

        public ApiResponse ToResponse()
        {
            return new ApiResponse(200, JsonConvert.SerializeObject(GetJsonRepresentation()));
        }
    }
}
