using ApiFramework.Enums;
using ApiFramework.Resources;
using Newtonsoft.Json.Linq;

namespace ApiFramework.Interfaces
{
    public interface IApiResource
    {
        public IApiItem RootItem { get; }
        public string GetResourceName();
        public EditPermission GetItemPermissions(string[] path);
        public void UpdateFrom(ApiResource other);
        public JToken ToJson();
        public string ToJsonString();
        public ApiResponse ToResponse();
    }
}
