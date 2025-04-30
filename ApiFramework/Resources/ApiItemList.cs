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
        private readonly List<IApiItem> _items;

        public ApiItemList(IApiItemContainer parent, bool canEdit) : base(parent, canEdit)
        {
            _items = new();
        }

        public int Count()
        {
            return _items.Count;
        }

        public bool Contains(IApiItem item)
        {
            return _items.Contains(item);
        }

        public string NameOf(IApiItem item)
        {
            if (!Contains(item)) throw new ArgumentException($"ApiItemList doesn't contain item {item}");
            return $"{GetParent().NameOf(this)}.{IndexOf(item)}";
        }

        public int IndexOf(IApiItem item)
        {
            if (!Contains(item)) throw new ArgumentException($"ApiItemList doesn't contain item {item}");
            return _items.IndexOf(item);
        }

        public void Insert(IApiItem item) => Insert(item, true);
        public void Insert(IApiItem item, bool checkCanEdit)
        {
            if (checkCanEdit && !CanEdit()) throw new ApiResourceItemReadOnlyException(ToString());
            _items.Add(item);
        }

        public void InsertValue<T>(T value, bool canEdit) => InsertValue(value, canEdit, true);
        public void InsertValue<T>(T value, bool canEdit, bool checkCanEdit)
        {
            if (checkCanEdit && !CanEdit()) throw new ApiResourceItemReadOnlyException(ToString());
            IApiItem item = new ApiItemValue<T>(this, canEdit, value);
            Insert(item);
        }

        public void Remove(IApiItem item) => Remove(item, true);
        public void Remove(IApiItem item, bool checkCanEdit)
        {
            if (checkCanEdit && !CanEdit()) throw new ApiResourceItemReadOnlyException(ToString());
            _items.Remove(item);
        }

        public void Clear() => Clear(true); 
        public void Clear(bool checkCanEdit)
        {
            if (checkCanEdit && !CanEdit()) throw new ApiResourceItemReadOnlyException(ToString());
            _items.Clear();
        }

        public IApiItem this[int index]
        {
            get
            {
                if (index >= _items.Count) throw new IndexOutOfRangeException("index");
                return _items[index];
            }
        }

        public override JToken ToJsonRepresentation()
        {
            return new JArray(_items.Select((item) => { return item.ToJsonRepresentation(); }));
        }
        public override IApiItem CreateCopy(IApiItemContainer container, bool canEdit)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            ApiItemList newList = new ApiItemList(container, canEdit);
            foreach (IApiItem item in _items)
            {
                IApiItem newItem = item.CreateCopy(newList, item.CanEdit());
                newList.Insert(newItem);
            }
            return newList;
        }

        public override void UpdateFrom(IApiItem other)
        {
            if (!CanEdit()) throw new ApiResourceItemReadOnlyException(ToString());
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (other is ApiItemList otherList)
            {
                Clear();
                foreach (IApiItem item in otherList)
                {
                    IApiItem newItem = item.CreateCopy(this, item.CanEdit());
                    Insert(newItem);
                }
            }
            else
            {
                throw new ArgumentException($"Can't update {GetType()} from item of different type {other.GetType()}");
            }
        }

        public IEnumerator<IApiItem> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}
