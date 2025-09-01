using ApiFramework.Enums;
using ApiFramework.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ApiFramework.Resources
{
    public class ApiItemList : ApiItem, IApiItemContainer, IEnumerable<IApiItem>
    {
        private List<IApiItem> Items { get; } = new();

        public ApiItemList(ApiPropertyInfo propertyInfo, IApiItemContainer parent) : base(propertyInfo, parent) { }

        public int Count()
        {
            return Items.Count;
        }

        public bool Contains(IApiItem item)
        {
            return Items.Contains(item);
        }

        public string NameOf(IApiItem item)
        {
            if (!Contains(item)) throw new ArgumentException($"ApiItemList doesn't contain item {item}");

            return IndexOf(item).ToString();
        }

        public int IndexOf(IApiItem item)
        {
            if (!Contains(item)) throw new ArgumentException($"ApiItemList doesn't contain item {item}");

            return Items.IndexOf(item);
        }

        public IApiItem this[int index]
        {
            get
            {
                if (index < 0 || index >= Items.Count) throw new IndexOutOfRangeException("index");

                return Items[index];
            }
        }

        public T? Get<T>(int index) where T : IApiItem
        {
            IApiItem item = this[index];
            return (item is T tItem) ? tItem : default;
        }

        public T InsertNew<T>(ApiPropertyInfo itemPropertyInfo) where T : IApiItem => InsertNew<T>(itemPropertyInfo, true);
        public T InsertNew<T>(ApiPropertyInfo itemPropertyInfo, bool checkPermissions) where T : IApiItem
        {
            if (itemPropertyInfo == null) throw new ArgumentNullException(nameof(itemPropertyInfo));
            if (!typeof(T).IsAssignableFrom(itemPropertyInfo.TargetType)) throw new ArgumentException($"Property type {itemPropertyInfo.TargetType.GetNiceTypeName()} is incompatible with generic argument type {typeof(T).GetNiceTypeName()}!");
            if (checkPermissions)
            {
                PropertyInfo.CheckPermissions(EditPermission.Modify);
                itemPropertyInfo.CheckPermissions(EditPermission.Create);
            }

            IApiItem newItem = (IApiItem)Activator.CreateInstance(itemPropertyInfo.TargetType, itemPropertyInfo, this);
            Items.Add(newItem);

            return (T)newItem;
        }

        public T InsertCopy<T>(ApiPropertyInfo itemPropertyInfo, T sourceItem) where T : IApiItem => InsertCopy<T>(itemPropertyInfo, sourceItem, true);
        public T InsertCopy<T>(ApiPropertyInfo itemPropertyInfo, T sourceItem, bool checkPermissions) where T : IApiItem
        {
            if (itemPropertyInfo == null) throw new ArgumentNullException(nameof(itemPropertyInfo));
            if (sourceItem == null) throw new ArgumentNullException(nameof(sourceItem));
            if (!typeof(T).IsAssignableFrom(itemPropertyInfo.TargetType)) throw new ArgumentException($"Property type {itemPropertyInfo.TargetType.GetNiceTypeName()} is incompatible with generic argument type {typeof(T).GetNiceTypeName()}!");
            if (checkPermissions) PropertyInfo.CheckPermissions(EditPermission.Modify);

            IApiItem newItem = sourceItem.CopyTo(itemPropertyInfo, this, checkPermissions);
            Items.Add(newItem);

            return (T)newItem;
        }

        public void Remove(IApiItem item) => Remove(item, true);
        public void Remove(IApiItem item, bool checkPermissions)
        {
            if (checkPermissions) PropertyInfo.CheckPermissions(EditPermission.Modify);

            Items.Remove(item);
        }

        public void Clear() => Clear(true); 
        public void Clear(bool checkPermissions)
        {
            if (checkPermissions) PropertyInfo.CheckPermissions(EditPermission.Modify);

            Items.Clear();
        }

        public override JToken ToJson()
        {
            return new JArray(Items.Select((item) => { return item.ToJson(); }));
        }

        public override IApiItem CopyTo(ApiPropertyInfo newPropertyInfo, IApiItemContainer newParent, bool checkPermissions)
        {
            if (newPropertyInfo == null) throw new ArgumentNullException(nameof(newPropertyInfo));
            if (newParent == null) throw new ArgumentNullException(nameof(newParent));
            if (checkPermissions) newPropertyInfo.CheckPermissions(EditPermission.Create);

            ApiItemList newList = new ApiItemList(newPropertyInfo, newParent);
            ApiPropertyInfo childPropertyInfo = PropertyInfo.Resource.PropertyInfos[newPropertyInfo.Path.Append("#")];
            foreach (IApiItem item in Items)
            {
                newList.InsertCopy(childPropertyInfo, item, checkPermissions);
            }
            return newList;
        }

        public override void UpdateFrom(IApiItem other, bool checkPermissions)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (other is not ApiItemList otherList) throw new ArgumentException($"Can't update {GetType()} from item of different type {other.GetType()}");
            if (checkPermissions) PropertyInfo.CheckPermissions(EditPermission.Modify);
            
            ApiPropertyInfo childPropertyInfo = PropertyInfo.Resource.PropertyInfos[PropertyInfo.Path.Append("#")];
            Clear(false);
            foreach (IApiItem item in otherList)
            {
                InsertCopy(childPropertyInfo, item, checkPermissions);
            }
        }

        public IEnumerator<IApiItem> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}
