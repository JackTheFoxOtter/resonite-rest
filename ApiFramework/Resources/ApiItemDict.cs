using ApiFramework.Enums;
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
        private Dictionary<string, IApiItem> ItemMapping { get; } = new();
        private Dictionary<IApiItem, string> ItemReverseMapping { get; } = new();

        public ApiItemDict(IApiItemContainer parent, EditPermission perms) : base(parent, perms) { }

        public int Count()
        {
            return ItemMapping.Count;
        }

        public bool Contains(IApiItem item)
        {
            return ItemReverseMapping.ContainsKey(item);
        }

        public bool ContainsKey(string key)
        {
            return ItemMapping.ContainsKey(key);
        }

        public string NameOf(IApiItem item)
        {
            if (!Contains(item)) throw new ArgumentException($"ApiItemDict doesn't contain item {item}");

            return ItemReverseMapping[item];
        }

        public void Insert(string key, IApiItem item) => Insert(key, item, true);
        public void Insert(string key, IApiItem item, bool checkCanModify)
        {
            if (checkCanModify && !CanModify) throw new ApiResourceItemReadOnlyException(ToString());
            if (ContainsKey(key)) throw new ArgumentException($"ApiItemDict already contains an item with key {key}");
            if (Contains(item)) throw new ArgumentException($"ApiItemDict already contains item {item}");

            ItemMapping.Add(key, item);
            ItemReverseMapping.Add(item, key);
        }

        public void InsertValue<T>(string key, T value, EditPermission perms) => InsertValue<T>(key, value, perms, true);
        public void InsertValue<T>(string key, T value, EditPermission perms, bool checkCanModify)
        {
            if (checkCanModify && !CanModify) throw new ApiResourceItemReadOnlyException(ToString());

            IApiItem item = new ApiItemValue<T>(this, perms, value);
            Insert(key, item);
        }

        public void Remove(string key) => Remove(key, true);
        public void Remove(string key, bool checkCanModify)
        {
            if (checkCanModify && !CanModify) throw new ApiResourceItemReadOnlyException(ToString());

            if (ContainsKey(key))
            {
                ItemReverseMapping.Remove(ItemMapping[key]);
                ItemMapping.Remove(key);
            }
        }

        public void Clear() => Clear(true);
        public void Clear(bool checkCanModify)
        {
            if (checkCanModify && !CanModify) throw new ApiResourceItemReadOnlyException(ToString());

            ItemReverseMapping.Clear();
            ItemMapping.Clear();
        }

        public IApiItem this[string key]
        {
            get
            {
                if (!ContainsKey(key)) throw new ArgumentException($"ApiItemDict doesn't contain an item with key {key}");

                return ItemMapping[key];
            }
        }

        public override JToken ToJson()
        {
            JObject jsonObj = new JObject();
            foreach (KeyValuePair<string, IApiItem> kv in ItemMapping)
            {
                jsonObj.Add(kv.Key, kv.Value.ToJson());
            }
            return jsonObj;
        }

        public override IApiItem CreateCopy(IApiItemContainer container, EditPermission perms)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            ApiItemDict newDict = new ApiItemDict(container, perms);
            foreach (KeyValuePair<string, IApiItem> kv in ItemMapping)
            {
                newDict.Insert(kv.Key, kv.Value.CreateCopy(newDict, kv.Value.Permissions));
            }
            return newDict;
        }

        public override void UpdateFrom(IApiItem other)
        {
            if (!CanModify) throw new ApiResourceItemReadOnlyException(ToString());
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
                        Insert(key, item.CreateCopy(this, item.Permissions));
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
            return ItemMapping.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ItemMapping.GetEnumerator();
        }
    }
}
