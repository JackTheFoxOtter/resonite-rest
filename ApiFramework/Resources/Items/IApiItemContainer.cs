namespace ApiFramework.Resources.Items
{
    /// <summary>
    /// API Item Containers can contain one or multiple API Items.
    /// They are used to represent hierarchical structures of API Items.
    /// </summary>
    public interface IApiItemContainer
    {
        /// <summary>
        /// Count of items in this container.
        /// </summary>
        /// <returns>Count of items contained.</returns>
        public int Count();

        /// <summary>
        /// Checks wether a specific item is contained in this container.
        /// </summary>
        /// <returns>Count of items contained.</returns>
        public bool Contains(IApiItem item);

        /// <summary>
        /// Returns the name representing an item contained in this container.
        /// Will throw an Exception if the item is not contained in this container.
        /// </summary>
        /// <param name="item">The item to retrieve the name of.</param>
        /// <returns>Name of the item.</returns>
        /// <exception cref="ArgumentException"></exception>
        public string NameOf(IApiItem item);

        /// <summary>
        /// Removes an item from this container.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        public void RemoveItem(IApiItem item);
    }
}
