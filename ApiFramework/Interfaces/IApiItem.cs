using ApiFramework.Resources;
using Newtonsoft.Json.Linq;

namespace ApiFramework.Interfaces
{
    public interface IApiItem
    {
        public ApiPropertyInfo PropertyInfo { get; }
        public IApiItemContainer Parent { get; }
        public IApiItem CopyTo(ApiPropertyInfo newPropertyInfo, IApiItemContainer newParent);
        public IApiItem CopyTo(ApiPropertyInfo newPropertyInfo, IApiItemContainer newParent, bool checkPermissions);
        public void UpdateFrom(IApiItem other);
        public void UpdateFrom(IApiItem other, bool checkPermissions);
        public JToken ToJson();
        public string ToJsonString();
        public ApiResponse ToResponse();
    }
}
