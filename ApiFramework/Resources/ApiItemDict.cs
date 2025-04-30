using ApiFramework.Exceptions;
using ApiFramework.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ApiFramework.Resources
{
    public class ApiItemDict : ApiItem, IApiItemContainer, IEnumerable<KeyValuePair<string, IApiItem>>
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

        public void Insert(string key, IApiItem item) => Insert(key, item, true);
        public void Insert(string key, IApiItem item, bool checkCanEdit)
        {
            if (checkCanEdit && !CanEdit()) throw new ApiResourceItemReadOnlyException(ToString());
            if (ContainsKey(key)) throw new ArgumentException($"ApiItemDict already contains an item with key {key}");
            if (Contains(item)) throw new ArgumentException($"ApiItemDict already contains item {item}");
            _itemMapping.Add(key, item);
            _itemReverseMapping.Add(item, key);
        }

        public void InsertValue<T>(string key, T value, bool canEdit) => InsertValue<T>(key, value, canEdit, true);
        public void InsertValue<T>(string key, T value, bool canEdit, bool checkCanEdit)
        {
            if (checkCanEdit && !CanEdit()) throw new ApiResourceItemReadOnlyException(ToString());
            IApiItem item = new ApiItemValue<T>(this, canEdit, value);
            Insert(key, item);
        }

        public void Remove(string key) => Remove(key, true);
        public void Remove(string key, bool checkCanEdit)
        {
            if (checkCanEdit && !CanEdit()) throw new ApiResourceItemReadOnlyException(ToString());
            if (ContainsKey(key))
            {
                _itemReverseMapping.Remove(_itemMapping[key]);
                _itemMapping.Remove(key);
            }
        }

        public void Clear() => Clear(true);
        public void Clear(bool checkCanEdit)
        {
            if (checkCanEdit && !CanEdit()) throw new ApiResourceItemReadOnlyException(ToString());
            _itemReverseMapping.Clear();
            _itemMapping.Clear();
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

        public override IApiItem CreateCopy(IApiItemContainer container, bool canEdit)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            ApiItemDict newDict = new ApiItemDict(container, canEdit);
            foreach (KeyValuePair<string, IApiItem> kv in _itemMapping)
            {
                newDict.Insert(kv.Key, kv.Value.CreateCopy(newDict, kv.Value.CanEdit()));
            }
            return newDict;
        }

        public override void UpdateFrom(IApiItem other)
        {
            if (!CanEdit()) throw new ApiResourceItemReadOnlyException(ToString());
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (other is ApiItemDict otherDict)
            {
                foreach (KeyValuePair<string, IApiItem> kv in otherDict)
                {
                    string key = kv.Key;
                    IApiItem item = kv.Value;
                    if (ContainsKey(key))
                    {
                        // Update existing item
                        this[key].UpdateFrom(item);
                    }
                    else
                    {
                        // Insert new item
                        Insert(key, item.CreateCopy(this, item.CanEdit()));
                    }
                }
            }
            else
            {
                throw new ArgumentException($"Can't update {GetType()} from item of different type {other.GetType()}");
            }
        }

        public IEnumerator<KeyValuePair<string, IApiItem>> GetEnumerator()
        {
            return _itemMapping.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _itemMapping.GetEnumerator();
        }
    }
}
