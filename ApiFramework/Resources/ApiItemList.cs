using ApiFramework.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiFramework.Resources
{
    public class ApiItemList : ApiItem, IApiItemContainer
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

        public void Insert(IApiItem item)
        {
            _items.Add(item);
        }

        public void Remove(IApiItem item)
        {
            _items.Remove(item);
        }

        public void InsertValue<T>(T value, bool canEdit)
        {
            IApiItem item = new ApiItemValue<T>(this, canEdit, value);
            Insert(item);
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
    }
}
