using ApiFramework.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ApiFramework.Resources
{
    public class ApiItemDict : ApiItem, IApiItemContainer
    {
        private Dictionary<string, IApiItem> _itemMapping;
        private Dictionary<IApiItem, string> _itemReverseMapping;

        public ApiItemDict(IApiItemContainer parent, bool canEdit) : base(parent, canEdit)
        {
            _itemMapping = new();
            _itemReverseMapping= new();
        }
        public int Count()
        {
            return _itemMapping.Count;
        }

        public bool Contains(IApiItem item)
        {
            return _itemReverseMapping.ContainsKey(item);
        }

        public bool ContainsKey(string key)
        {
            return _itemMapping.ContainsKey(key);
        }

        public string NameOf(IApiItem item)
        {
            if (!Contains(item)) throw new ArgumentException($"ApiItemDict doesn't contain item {item}");
            return _itemReverseMapping[item];
        }

        public void Insert(string key, IApiItem item)
        {
            if (ContainsKey(key)) throw new ArgumentException($"ApiItemDict already contains an item with key {key}");
            if (Contains(item)) throw new ArgumentException($"ApiItemDict already contains item {item}");
            _itemMapping.Add(key, item);
            _itemReverseMapping.Add(item, key);
        }

        public void Remove(string key)
        {
            if (ContainsKey(key))
            {
                _itemReverseMapping.Remove(_itemMapping[key]);
                _itemMapping.Remove(key);
            }
        }

        public void InsertValue<T>(string key, T value, bool canEdit)
        {
            IApiItem item = new ApiItemValue<T>(this, canEdit, value);
            Insert(key, item);
        }

        public IApiItem this[string key]
        {
            get
            {
                if (!ContainsKey(key)) throw new ArgumentException($"ApiItemDict doesn't contain an item with key {key}");
                return _itemMapping[key];
            }
        }

        public override JToken ToJsonRepresentation()
        {
            //Dictionary<string, JToken> jsonDict = _itemMapping.ToDictionary(kv => kv.Key, kv => kv.Value.ToJsonRepresentation());
            JObject jsonObj = new JObject();
            foreach (KeyValuePair<string, IApiItem> kv in _itemMapping)
            {
                jsonObj.Add(kv.Key, kv.Value.ToJsonRepresentation());
            }
            return jsonObj;
            //return _itemMapping.ToDictionary(kv => kv.Key, kv => kv.Value.ToJsonRepresentation()));
        }
    }
}
