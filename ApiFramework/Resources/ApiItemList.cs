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

        public ApiItemList(EditPermission perms) : base(perms) { }
        public ApiItemList(EditPermission perms, IApiItemContainer? parent) : base(perms, parent) { }

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

            return $"{Parent.NameOf(this)}.{IndexOf(item)}";
        }

        public int IndexOf(IApiItem item)
        {
            if (!Contains(item)) throw new ArgumentException($"ApiItemList doesn't contain item {item}");

            return Items.IndexOf(item);
        }

        public void Insert(IApiItem item) => Insert(item, true);
        public void Insert(IApiItem item, bool checkPermissions)
        {
            if (checkPermissions) CheckPermissions(EditPermission.Modify);

            Items.Add(item);
            item.SetParent(this);
        }

        public void InsertValue<T>(T value, EditPermission perms) => InsertValue(value, perms, true);
        public void InsertValue<T>(T value, EditPermission perms, bool checkPermissions)
        {
            if (checkPermissions) CheckPermissions(EditPermission.Modify);

            Insert(new ApiItemValue<T>(perms, this, value));
        }

        public void Remove(IApiItem item) => Remove(item, true);
        public void Remove(IApiItem item, bool checkPermissions)
        {
            if (checkPermissions) CheckPermissions(EditPermission.Modify);

            Items.Remove(item);
        }

        public void Clear() => Clear(true); 
        public void Clear(bool checkPermissions)
        {
            if (checkPermissions) CheckPermissions(EditPermission.Modify);

            Items.Clear();
        }

        public IApiItem this[int index]
        {
            get
            {
                if (index >= Items.Count) throw new IndexOutOfRangeException("index");

                return Items[index];
            }
        }

        public override JToken ToJson()
        {
            return new JArray(Items.Select((item) => { return item.ToJson(); }));
        }

        public override IApiItem CreateCopy(IApiItemContainer container, EditPermission perms)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            ApiItemList newList = new ApiItemList(perms, container);
            foreach (IApiItem item in Items)
            {
                IApiItem newItem = item.CreateCopy(newList, perms);
                newList.Insert(newItem);
            }
            return newList;
        }

        public override void UpdateFrom(IApiItem other, bool checkPermissions)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (other is not ApiItemList otherList) throw new ArgumentException($"Can't update {GetType()} from item of different type {other.GetType()}");
            if (checkPermissions) CheckPermissions(EditPermission.Modify);
            
            Clear();
            foreach (IApiItem item in otherList)
            {
                IApiItem newItem = item.CreateCopy(this, item.Permissions);
                Insert(newItem);
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
