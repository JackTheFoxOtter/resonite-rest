using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace ApiFramework.Resources
{
    public class ApiItemList : ApiItem, IApiItemContainer, IEnumerable<IApiItem>
    {
        private List<IApiItem> Items { get; } = new();

        public ApiItemList() { }

        public ApiItemList(IApiItemContainer? parent) : base(parent) { }

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

        public void RemoveItem(IApiItem item)
        {
            Items.Remove(item);
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

        public void Insert(IApiItem item)
        {
            Items.Add(item);
            item.SetParent(this);
        }

        public T InsertNew<T>() where T : IApiItem, new()
        {
            T newItem = Activator.CreateInstance<T>();
            newItem.SetParent(this);
            Items.Add(newItem);
            return newItem;
        }

        public T InsertCopy<T>(T sourceItem) where T : IApiItem
        {
            if (sourceItem == null) throw new ArgumentNullException(nameof(sourceItem));
            
            T copiedItem = (T)sourceItem.CreateCopy();
            copiedItem.SetParent(this);
            Items.Add(copiedItem);
            return copiedItem;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Items.Count) throw new IndexOutOfRangeException("index");

            Items.RemoveAt(index);
        }

        public void Clear()
        {
            Items.Clear();
        }

        public override JsonNode ToJson()
        {
            return new JsonArray(Items.Select((item) => { return item.ToJson(); }).ToArray());
        }

        public override void UpdateFrom(IApiItem other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (other is not ApiItemList otherList) throw new ArgumentException($"Can't update {GetType()} from item of different type {other.GetType()}");

            Clear();
            foreach (IApiItem item in otherList)
            {
                InsertCopy(item);
            }
        }

        public override IApiItem CreateCopy()
        {
            ApiItemList copy = new ApiItemList();
            foreach (IApiItem item in Items)
            {
                copy.InsertCopy(item);
            }
            return copy;
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
