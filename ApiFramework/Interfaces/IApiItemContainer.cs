namespace ApiFramework.Interfaces
{
    public interface IApiItemContainer
    {
        public bool CanEdit();
        public int Count();
        public bool Contains(IApiItem item);
        public string NameOf(IApiItem item);
    }
}
