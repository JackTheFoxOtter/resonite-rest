namespace ApiFramework.Interfaces
{
    public interface IApiItemContainer
    {
        public IApiResource? Resource { get; }
        public int Count();
        public bool Contains(IApiItem item);
        public string NameOf(IApiItem item);
    }
}
