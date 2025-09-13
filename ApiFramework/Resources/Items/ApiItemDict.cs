using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace ApiFramework.Resources.Items
{
    /// <summary>
    /// API Item to represent a key-value pair of items.
    /// Item keys are always strings.
    /// </summary>
    public class ApiItemDict : ApiItem, IApiItemContainer, IEnumerable<KeyValuePair<string, IApiItem>>
    {
        private Dictionary<string, IApiItem> ItemMapping { get; } = new();
        private Dictionary<IApiItem, string> ItemReverseMapping { get; } = new();

        /// <summary>
        /// Creates a new ApiItemDict instance.
        /// </summary>
        public ApiItemDict() : base() { }

        /// <summary>
        /// Creates a new ApiItemDict instance.
        /// </summary>
        /// <param name="parent">The container to assign the item to.</param>
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

        /// <summary>
        /// Checks whether this dictionary contains an item with a specific key.
        /// </summary>
        /// <param name="key">The key to check for.</param>
        /// <returns>True if the key is contained in the dictionary.</returns>
        public bool ContainsKey(string key)
        {
            return ItemMapping.ContainsKey(key);
        }

        /// <summary>
        /// Retrieves an item at a key in this dictionary.
        /// </summary>
        /// <param name="key">Key of the item to retrieve.</param>
        /// <returns>The item with the specified key.</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
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

        /// <summary>
        /// Retrieves an item at a key in this dictionary.
        /// </summary>
        /// <typeparam name="T">Type of the item to retrieve.</typeparam>
        /// <param name="key">Key of the item to retrieve.</param>
        /// <returns>The item with the specified key.</returns>
        public T? Get<T>(string key) where T : IApiItem
        {
            IApiItem item = this[key];
            return item is T tItem ? tItem : default;
        }

        /// <summary>
        /// Inserts an item into the dictionary.
        /// </summary>
        /// <param name="key">The key to add the item with.</param>
        /// <param name="item">The item to add.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void Insert(string key, IApiItem item)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (ContainsKey(key)) throw new ArgumentException($"ApiItemDict already contains an item with key {key}");

            ItemMapping.Add(key, item);
            ItemReverseMapping.Add(item, key);
            item.SetParent(this);
        }

        /// <summary>
        /// Creates a new item and inserts it into this dictionary.
        /// </summary>
        /// <typeparam name="T">Type of the item to create.</typeparam>
        /// <param name="key">The key to add the item with.</param>
        /// <returns>The newly created item.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
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

        /// <summary>
        /// Creates a copy of an item and inserts it into this dictionary.
        /// The source item remains unchanged.
        /// </summary>
        /// <typeparam name="T">The type of the item to duplicate.</typeparam>
        /// <param name="key">The key to add the item with.</param>
        /// <param name="sourceItem">The item to duplicate.</param>
        /// <returns>The newly created item.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
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

        /// <summary>
        /// Removes an item at a key from this dictionary.
        /// </summary>
        /// <param name="key">Key of the item to remove.</param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void Remove(string key)
        {
            if (ContainsKey(key))
            {
                ItemReverseMapping.Remove(ItemMapping[key]);
                ItemMapping.Remove(key);
            }
        }

        /// <summary>
        /// Clears this dictionary of all items.
        /// </summary>
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
