using ApiFramework.Resources;

namespace ApiFramework.Interfaces
{
    public interface IApiItemContainer
    {
        public ApiPropertyInfo PropertyInfo { get; }
        public int Count();
        public bool Contains(IApiItem item);
        public string NameOf(IApiItem item);
    }
}
