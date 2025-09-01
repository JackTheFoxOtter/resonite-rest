using ApiFramework.Enums;
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

        public ApiItemDict(ApiPropertyInfo propertyInfo, IApiItemContainer parent) : base(propertyInfo, parent) { }

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

        public T InsertNew<T>(string key, ApiPropertyInfo itemPropertyInfo) => InsertNew<T>(key, itemPropertyInfo, true);
        public T InsertNew<T>(string key, ApiPropertyInfo itemPropertyInfo, bool checkPermissions)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (itemPropertyInfo == null) throw new ArgumentNullException(nameof(itemPropertyInfo));
            if (!typeof(T).IsAssignableFrom(itemPropertyInfo.TargetType)) throw new ArgumentException($"Property type {itemPropertyInfo.TargetType.GetNiceTypeName()} is incompatible with generic argument type {typeof(T).GetNiceTypeName()}!");
            if (ContainsKey(key)) throw new ArgumentException($"ApiItemDict already contains an item with key {key}");
            if (checkPermissions)
            {
                PropertyInfo.CheckPermissions(EditPermission.Modify);
                itemPropertyInfo.CheckPermissions(EditPermission.Create);
            }

            IApiItem newItem = (IApiItem)Activator.CreateInstance(itemPropertyInfo.TargetType, itemPropertyInfo, this);
            ItemMapping.Add(key, newItem);
            ItemReverseMapping.Add(newItem, key);

            return (T)newItem;
        }

        public T InsertCopy<T>(string key, ApiPropertyInfo itemPropertyInfo, T sourceItem) where T : IApiItem => InsertCopy<T>(key, itemPropertyInfo, sourceItem, true);
        public T InsertCopy<T>(string key, ApiPropertyInfo itemPropertyInfo, T sourceItem, bool checkPermissions) where T : IApiItem
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (itemPropertyInfo == null) throw new ArgumentNullException(nameof(itemPropertyInfo));
            if (!typeof(T).IsAssignableFrom(itemPropertyInfo.TargetType)) throw new ArgumentException($"Property type {itemPropertyInfo.TargetType.GetNiceTypeName()} is incompatible with generic argument type {typeof(T).GetNiceTypeName()}!");
            if (ContainsKey(key)) throw new ArgumentException($"ApiItemDict already contains an item with key {key}");
            if (checkPermissions) PropertyInfo.CheckPermissions(EditPermission.Modify);

            IApiItem newItem = sourceItem.CopyTo(itemPropertyInfo, this, checkPermissions);
            ItemMapping.Add(key, newItem);
            ItemReverseMapping.Add(newItem, key);

            return (T)newItem;
        }

        public void Remove(string key) => Remove(key, true);
        public void Remove(string key, bool checkPermissions)
        {
            if (checkPermissions) PropertyInfo.CheckPermissions(EditPermission.Modify);

            if (ContainsKey(key))
            {
                ItemReverseMapping.Remove(ItemMapping[key]);
                ItemMapping.Remove(key);
            }
        }

        public void Clear() => Clear(true);
        public void Clear(bool checkPermissions)
        {
            if (checkPermissions) PropertyInfo.CheckPermissions(EditPermission.Modify);

            ItemReverseMapping.Clear();
            ItemMapping.Clear();
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

        public override IApiItem CopyTo(ApiPropertyInfo newPropertyInfo, IApiItemContainer newParent, bool checkPermissions)
        {
            if (newPropertyInfo == null) throw new ArgumentNullException(nameof(newPropertyInfo));
            if (newParent == null) throw new ArgumentNullException(nameof(newParent));

            ApiItemDict newDict = new ApiItemDict(newPropertyInfo, newParent);
            foreach (KeyValuePair<string, IApiItem> kv in ItemMapping)
            {
                ApiPropertyInfo childPropertyInfo = PropertyInfo.Resource.PropertyInfos[newPropertyInfo.Path.Append(kv.Key)];
                newDict.InsertCopy(kv.Key, childPropertyInfo, kv.Value, checkPermissions);
            }
            return newDict;
        }

        public override void UpdateFrom(IApiItem other, bool checkPermissions)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (other is not ApiItemDict otherDict) throw new ArgumentException($"Can't update {GetType()} from item of different type {other.GetType()}");
            if (checkPermissions) PropertyInfo.CheckPermissions(EditPermission.Modify);

            foreach (KeyValuePair<string, IApiItem> kv in otherDict)
            {
                if (ContainsKey(kv.Key))
                {
                    // Update existing item
                    this[kv.Key].UpdateFrom(kv.Value, false); // Permissions already checked above
                }
                else
                {
                    // Insert new item
                    ApiPropertyInfo childPropertyInfo = PropertyInfo.Resource.PropertyInfos[PropertyInfo.Path.Append(kv.Key)];
                    InsertCopy(kv.Key, childPropertyInfo, kv.Value, checkPermissions);
                }
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
