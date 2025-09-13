using ApiFramework.Exceptions;
using System.Text.Json.Nodes;

namespace ApiFramework.Resources.Items
{
    /// <summary>
    /// API Items represent individual nodes within a data structure.
    /// They can be assigned to a parent container to represent hierarchical structures,
    /// and provide functionality to convert to / from JSON.
    /// </summary>
    public interface IApiItem
    {
        /// <summary>
        /// The current parent container this item is assigned to, if any.
        /// </summary>
        public IApiItemContainer? Parent { get; }

        /// <summary>
        /// Assigns this item to a parent container, or unassigns it from the current one.
        /// If the item is already assigned to a parent container, it will first be removed from it.
        /// <br/><b>Attention: Usually this shouldn't be called directly.</b> Instead, use the corresponding functions on the target container to assign the item.
        /// That way ensures that the item is correctly registered with the target container.
        /// </summary>
        /// <param name="newParent">The target container to assign the item to or <i>null</i> to unassign.</param>
        public void SetParent(IApiItemContainer newParent);

        /// <summary>
        /// Updates / replaces the content of this item with that of a different item of the same type.
        /// </summary>
        /// <param name="other">The item to source the content from.</param>
        public void UpdateFrom(IApiItem other);

        /// <summary>
        /// Creates a deep copy of this item.
        /// </summary>
        /// <returns>Copied item instance of the same type.</returns>
        public IApiItem CreateCopy();

        /// <summary>
        /// Creates a JSON-Representation for this item instance.
        /// </summary>
        /// <returns>JSON-Representation of this item.</returns>
        public JsonNode ToJson();

        /// <summary>
        /// Creates a string with a JSON-Representation of this item instance.
        /// </summary>
        /// <returns>String with JSON-Representation of this item.</returns>
        public string ToJsonString() => ToJson()?.ToJsonString() ?? throw new ApiJsonParsingException("Failed to convert ApiItem to JSON-String!");

        /// <summary>
        /// Creates an API Response ( 200 - OK ) with a JSON-Representation of this item instance.
        /// </summary>
        /// <returns>API-Response with JSON-Representation of this item.</returns>
        public ApiResponse ToResponse() => new ApiResponse(200, ToJsonString());
    }
}
