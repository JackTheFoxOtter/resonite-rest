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

        public ApiItemDict(EditPermission perms) : base(perms) { }
        public ApiItemDict(EditPermission perms, IApiItemContainer? parent) : base(perms, parent) { }

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
        public void Insert(string key, IApiItem item, bool checkPermission)
        {
            if (ContainsKey(key)) throw new ArgumentException($"ApiItemDict already contains an item with key {key}");
            if (Contains(item)) throw new ArgumentException($"ApiItemDict already contains item {item}");
            if (checkPermission) CheckPermissions(EditPermission.Modify);

            ItemMapping.Add(key, item);
            ItemReverseMapping.Add(item, key);
            item.SetParent(this);
        }

        public void Remove(string key) => Remove(key, true);
        public void Remove(string key, bool checkCanModify)
        {
            if (checkCanModify) CheckPermissions(EditPermission.Modify);

            if (ContainsKey(key))
            {
                ItemReverseMapping.Remove(ItemMapping[key]);
                ItemMapping.Remove(key);
            }
        }

        public void Clear() => Clear(true);
        public void Clear(bool checkCanModify)
        {
            if (checkCanModify) CheckPermissions(EditPermission.Modify);

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

            ApiItemDict newDict = new ApiItemDict(perms, container);
            foreach (KeyValuePair<string, IApiItem> kv in ItemMapping)
            {
                newDict.Insert(kv.Key, kv.Value.CreateCopy(newDict, kv.Value.Permissions));
            }
            return newDict;
        }

        public override void UpdateFrom(IApiItem other, bool checkPermissions)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (checkPermissions) CheckPermissions(EditPermission.Modify);

            if (other is ApiItemDict otherDict)
            {
                foreach (KeyValuePair<string, IApiItem> kv in otherDict)
                {
                    string key = kv.Key;
                    IApiItem item = kv.Value;
                    if (ContainsKey(key))
                    {
                        // Update existing item
                        this[key].UpdateFrom(item, false); // Permissions already checked above
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
