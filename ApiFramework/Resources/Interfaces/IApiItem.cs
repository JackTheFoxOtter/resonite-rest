using ApiFramework.Resources;
using Newtonsoft.Json.Linq;

namespace ApiFramework.Resources
{
    /// <summary>
    /// API Items represent individual nodes within a data structure.
    /// They are always contained within a API Item Container.
    /// </summary>
    public interface IApiItem
    {
        public IApiItemContainer Parent { get; }
        public IApiItem CopyTo(IApiItemContainer newParent);
        public void UpdateFrom(IApiItem other);
        public JToken ToJson();
        public string ToJsonString();
        public ApiResponse ToResponse();
    }
}
