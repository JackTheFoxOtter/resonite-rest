using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace ApiFramework.Resources.Items
{
    /// <summary>
    /// API Item to represent a list of items.
    /// </summary>
    public class ApiItemList : ApiItem, IApiItemContainer, IEnumerable<IApiItem>
    {
        private List<IApiItem> Items { get; } = new();

        /// <summary>
        /// Creates a new ApiItemList instance.
        /// </summary>
        public ApiItemList() { }

        /// <summary>
        /// Creates a new ApiItemList instance.
        /// </summary>
        /// <param name="parent">The container to assign the item to.</param>
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

        /// <summary>
        /// Returns the index of an item in this list.
        /// Throws an exception if the item is not contained in this list.
        /// </summary>
        /// <param name="item">The item to retrieve the index of.</param>
        /// <returns>Index of the item in the list.</returns>
        /// <exception cref="ArgumentException"></exception>
        public int IndexOf(IApiItem item)
        {
            if (!Contains(item)) throw new ArgumentException($"ApiItemList doesn't contain item {item}");

            return Items.IndexOf(item);
        }

        /// <summary>
        /// Retrieves an item at an index in this list.
        /// </summary>
        /// <param name="index">Index of the item to retrieve.</param>
        /// <returns>The item at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public IApiItem this[int index]
        {
            get
            {
                if (index < 0 || index >= Items.Count) throw new IndexOutOfRangeException("index");

                return Items[index];
            }
        }

        /// <summary>
        /// Retrieves an item at an index in this list.
        /// </summary>
        /// <typeparam name="T">Type of the item to retrieve.</typeparam>
        /// <param name="index">Index of the item to retrieve.</param>
        /// <returns>The item at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public T? Get<T>(int index) where T : IApiItem
        {
            IApiItem item = this[index];
            return item is T tItem ? tItem : default;
        }

        /// <summary>
        /// Inserts an item into this list.
        /// Will automatically register the list as the items (new) parent.
        /// </summary>
        /// <param name="item">The item to insert.</param>
        public void Insert(IApiItem item)
        {
            Items.Add(item);
            item.SetParent(this);
        }

        /// <summary>
        /// Creates a new item and inserts it into this list.
        /// </summary>
        /// <typeparam name="T">Type of item to create.</typeparam>
        /// <returns>The newly created item.</returns>
        public T InsertNew<T>() where T : IApiItem, new()
        {
            T newItem = Activator.CreateInstance<T>();
            newItem.SetParent(this);
            Items.Add(newItem);
            return newItem;
        }

        /// <summary>
        /// Creates a copy of an item and inserts it into this list.
        /// The source item remains unchanged.
        /// </summary>
        /// <typeparam name="T">The type of the item to duplicate.</typeparam>
        /// <param name="sourceItem">The item to duplicate.</param>
        /// <returns>The newly created item.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public T InsertCopy<T>(T sourceItem) where T : IApiItem
        {
            if (sourceItem == null) throw new ArgumentNullException(nameof(sourceItem));
            
            T copiedItem = (T)sourceItem.CreateCopy();
            copiedItem.SetParent(this);
            Items.Add(copiedItem);
            return copiedItem;
        }

        /// <summary>
        /// Removes an item at an index from this list.
        /// </summary>
        /// <param name="index">Index of the item to remove.</param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Items.Count) throw new IndexOutOfRangeException("index");

            Items.RemoveAt(index);
        }

        /// <summary>
        /// Clears this list of all items.
        /// </summary>
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
