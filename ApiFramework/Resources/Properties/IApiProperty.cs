using System.Text.Json.Nodes;
using ApiFramework.Enums;
using ApiFramework.Exceptions;
using ApiFramework.Resources.Items;

namespace ApiFramework.Resources.Properties
{
    /// <summary>
    /// API Properties are API Items that are associated with a defined structure.
    /// As part of that defined structure, they contain the following additional information:
    /// - What resource they belong to
    /// - Where they are in the structure (API Property Path)
    /// - What Edit Permission they have
    /// </summary>
    public interface IApiProperty<T> where T : IApiItem
    {
        public T Item { get; }
        public IApiResource Resource { get; }
        public ApiPropertyPath Path { get; }
        public EditPermission Permission { get; }
        public IApiProperty<T> CreateCopy();
        public JsonNode ToJson();
        public string ToJsonString() => ToJson()?.ToJsonString() ?? throw new ApiJsonParsingException("Failed to convert ApiProperty to JSON-String!");
        public ApiResponse ToResponse() => new ApiResponse(200, ToJsonString());
    }
}
