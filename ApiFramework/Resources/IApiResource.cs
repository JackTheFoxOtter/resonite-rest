using System.Text.Json.Nodes;

namespace ApiFramework.Resources
{
    /// <summary>
    /// API Resources 
    /// </summary>
    public interface IApiResource
    {
        public string ResourceName { get; }
        public IApiProperty RootProperty { get; }
        public void UpdateFrom(IApiResource other);
        public JsonNode ToJson() => RootProperty.Item.ToJson();
        public string ToJsonString() => RootProperty.Item.ToJsonString();
        public ApiResponse ToResponse() => RootProperty.Item.ToResponse();
    }
}
