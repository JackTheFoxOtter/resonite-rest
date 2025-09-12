using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace ApiFramework.Resources
{
    public class ApiItemDict : ApiItem, IApiItemContainer, IEnumerable<KeyValuePair<string, IApiItem>>
    {
        private Dictionary<string, IApiItem> ItemMapping { get; } = new();
        private Dictionary<IApiItem, string> ItemReverseMapping { get; } = new();

        public ApiItemDict() : base() { }

        public ApiItemDict(IApiItemContainer? parent) : base(parent) { }

        public int Count()
        {
            return ItemMapping.Count;
        }

        public bool Contains(IApiItem item)
        {
            return ItemReverseMapping.ContainsKey(item);
        }

        public string NameOf(IApiItem item)
        {
            if (!Contains(item)) throw new ArgumentException($"ApiItemDict doesn't contain item {item}");

            return ItemReverseMapping[item];
        }

        public void RemoveItem(IApiItem item)
        {
            if (Contains(item))
            {
                ItemMapping.Remove(ItemReverseMapping[item]);
                ItemReverseMapping.Remove(item);
            }
        }

        public bool ContainsKey(string key)
        {
            return ItemMapping.ContainsKey(key);
        }

        public IApiItem this[string key]
        {
            get
            {
                if (!ContainsKey(key)) throw new ArgumentException($"ApiItemDict doesn't contain an item with key {key}");

                return ItemMapping[key];
            }
            set
            {
                if (!ContainsKey(key)) throw new ArgumentException($"ApiItemDict doesn't contain an item with key {key}");

                ItemMapping[key] = value;
            }
        }

        public T? Get<T>(string key) where T : IApiItem
        {
            IApiItem item = this[key];
            return (item is T tItem) ? tItem : default;
        }

        public void Insert(string key, IApiItem item)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (ContainsKey(key)) throw new ArgumentException($"ApiItemDict already contains an item with key {key}");

            ItemMapping.Add(key, item);
            ItemReverseMapping.Add(item, key);
            item.SetParent(this);
        }
        
        public T InsertNew<T>(string key) where T : IApiItem, new()
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (ContainsKey(key)) throw new ArgumentException($"ApiItemDict already contains an item with key {key}");
            
            T newItem = Activator.CreateInstance<T>();
            newItem.SetParent(this);
            ItemMapping.Add(key, newItem);
            ItemReverseMapping.Add(newItem, key);
            return newItem;
        }

        public T InsertCopy<T>(string key, T sourceItem) where T : IApiItem
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (ContainsKey(key)) throw new ArgumentException($"ApiItemDict already contains an item with key {key}");
            
            T copiedItem = (T)sourceItem.CreateCopy();
            copiedItem.SetParent(this);
            ItemMapping.Add(key, copiedItem);
            ItemReverseMapping.Add(copiedItem, key);
            return copiedItem;
        }

        public void Remove(string key)
        {
            if (ContainsKey(key))
            {
                ItemReverseMapping.Remove(ItemMapping[key]);
                ItemMapping.Remove(key);
            }
        }

        public void Clear()
        {
            ItemReverseMapping.Clear();
            ItemMapping.Clear();
        }

        public override JsonNode ToJson()
        {
            JsonObject jsonObj = new JsonObject();
            foreach (KeyValuePair<string, IApiItem> kv in ItemMapping)
            {
                jsonObj.Add(kv.Key, kv.Value.ToJson());
            }
            return jsonObj;
        }

        public override void UpdateFrom(IApiItem other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (other is not ApiItemDict otherDict) throw new ArgumentException($"Can't update {GetType()} from item of different type {other.GetType()}");
            
            foreach (KeyValuePair<string, IApiItem> kv in otherDict)
            {
                if (ContainsKey(kv.Key))
                {
                    // Update existing item
                    this[kv.Key].UpdateFrom(kv.Value);
                }
                else
                {
                    // Insert new item
                    InsertCopy(kv.Key, kv.Value);
                }
            }
        }

        public override IApiItem CreateCopy()
        {
            ApiItemDict newDict = new ApiItemDict();
            foreach (KeyValuePair<string, IApiItem> kv in ItemMapping)
            {
                newDict.InsertCopy(kv.Key, kv.Value);
            }
            return newDict;
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
