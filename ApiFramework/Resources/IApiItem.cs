using ApiFramework.Exceptions;
using System.Text.Json.Nodes;

namespace ApiFramework.Resources
{
    /// <summary>
    /// API Items represent individual nodes within a data structure.
    /// </summary>
    public interface IApiItem
    {
        public IApiItemContainer? Parent { get; }
        public void SetParent(IApiItemContainer newParent);
        public void UpdateFrom(IApiItem other);
        public IApiItem CreateCopy();
        public JsonNode ToJson();
        public string ToJsonString() => ToJson()?.ToJsonString() ?? throw new ApiJsonParsingException("Failed to convert ApiItem to JSON-String!");
        public ApiResponse ToResponse() => new ApiResponse(200, ToJsonString());
    }
}
