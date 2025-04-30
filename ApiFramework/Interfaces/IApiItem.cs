using Newtonsoft.Json.Linq;

namespace ApiFramework.Interfaces
{
    public interface IApiItem
    {
        public IApiItemContainer GetParent();
        public bool CanEdit();
        public JToken ToJsonRepresentation();
        public string ToJson();
        public ApiResponse ToResponse();
        public IApiItem CreateCopy(IApiItemContainer container, bool canEdit);
        public void UpdateFrom(IApiItem other);
    }
}
