using ApiFramework.Enums;
using Newtonsoft.Json.Linq;

namespace ApiFramework.Interfaces
{
    public interface IApiItem
    {
        public IApiItemContainer Parent { get; }
        public EditPermission Permissions { get; }
        public bool CanCreate { get; }
        public bool CanModify { get; }
        public bool CanDelete { get; }
        public IApiItem CreateCopy(IApiItemContainer container, EditPermission permission);
        public void UpdateFrom(IApiItem other);
        public JToken ToJson();
        public string ToJsonString();
        public ApiResponse ToResponse();
    }
}
