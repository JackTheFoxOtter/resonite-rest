using ApiFramework.Enums;
using Newtonsoft.Json.Linq;

namespace ApiFramework.Interfaces
{
    public interface IApiItem
    {
        public IApiResource? Resource { get; }
        public IApiItemContainer? Parent { get; }
        public EditPermission Permissions { get; }
        public void SetParent(IApiItemContainer parent);
        public IApiItem CreateCopy(IApiItemContainer container, EditPermission permission);
        public void UpdateFrom(IApiItem other, bool checkPermissions);
        public JToken ToJson();
        public string ToJsonString();
        public ApiResponse ToResponse();
    }
}
