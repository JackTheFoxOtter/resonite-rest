using ApiFramework.Enums;
using ApiFramework.Exceptions;
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

        public ApiItemList(IApiItemContainer parent, EditPermission perms) : base(parent, perms) { }

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
        public void Insert(IApiItem item, bool checkCanModify)
        {
            if (checkCanModify && !CanModify) throw new ApiResourceItemReadOnlyException(ToString());

            Items.Add(item);
        }

        public void InsertValue<T>(T value, EditPermission perms) => InsertValue(value, perms, true);
        public void InsertValue<T>(T value, EditPermission perms, bool checkCanModify)
        {
            if (checkCanModify && !CanModify) throw new ApiResourceItemReadOnlyException(ToString());

            IApiItem item = new ApiItemValue<T>(this, perms, value);
            Insert(item);
        }

        public void Remove(IApiItem item) => Remove(item, true);
        public void Remove(IApiItem item, bool checkCanEdit)
        {
            if (checkCanEdit && !CanModify) throw new ApiResourceItemReadOnlyException(ToString());

            Items.Remove(item);
        }

        public void Clear() => Clear(true); 
        public void Clear(bool checkCanEdit)
        {
            if (checkCanEdit && !CanModify) throw new ApiResourceItemReadOnlyException(ToString());

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

            ApiItemList newList = new ApiItemList(container, perms);
            foreach (IApiItem item in Items)
            {
                IApiItem newItem = item.CreateCopy(newList, perms);
                newList.Insert(newItem);
            }
            return newList;
        }

        public override void UpdateFrom(IApiItem other)
        {
            if (!CanModify) throw new ApiResourceItemReadOnlyException(ToString());
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (other is not ApiItemList otherList) throw new ArgumentException($"Can't update {GetType()} from item of different type {other.GetType()}");
            
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
