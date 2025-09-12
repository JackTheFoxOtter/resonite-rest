namespace ApiFramework.Resources
{
    /// <summary>
    /// API Item Containers bundle multiple API Items into one logical group.
    /// </summary>
    public interface IApiItemContainer
    {
        public int Count();
        public bool Contains(IApiItem item);
        public string NameOf(IApiItem item);
        public void RemoveItem(IApiItem item);
    }
}
