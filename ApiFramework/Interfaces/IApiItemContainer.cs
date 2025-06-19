namespace ApiFramework.Interfaces
{
    public interface IApiItemContainer
    {
        public int Count();
        public bool Contains(IApiItem item);
        public string NameOf(IApiItem item);
    }
}
