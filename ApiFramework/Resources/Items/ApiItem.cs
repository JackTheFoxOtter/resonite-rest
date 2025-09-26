using ApiFramework.Exceptions;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ApiFramework.Resources.Items
{
    /// <summary>
    /// Abstract base class for all types of API Items.
    /// API Items represent individual nodes within a data structure.
    /// They can be assigned to a parent container to represent hierarchical structures,
    /// and provide functionality to convert to / from JSON.
    /// </summary>
    public abstract class ApiItem : IApiItem
    {
        public IApiItemContainer? Parent { get; private set; }

        /// <summary>
        /// Creates a new instance of the API item.
        /// </summary>
        public ApiItem() { }

        /// <summary>
        /// Creates a new instance of the API item.
        /// </summary>
        /// <param name="parent">The container to assign the item to.</param>
        public ApiItem(IApiItemContainer? parent) { SetParent(parent); }

        public void SetParent(IApiItemContainer? newParent)
        {
            if (Parent == newParent) return;

            Parent?.RemoveItem(this);
            Parent = newParent;
        }
        
        public abstract void UpdateFrom(IApiItem other);

        public abstract IApiItem CreateCopy();
        
        public abstract JsonNode ToJson();

        public override string ToString()
        {
            if (Parent != null && Parent.Contains(this))
            {
                if (Parent is ApiItem)
                {
                    return $"{Parent}.{Parent.NameOf(this)}";
                }
                else
                {
                    return Parent.NameOf(this);
                }
            }

            return $"<Unbound: {GetType().GetNiceTypeName()}>";
        }

        /// <summary>
        /// Creates an API Item from a JSON-String.
        /// </summary>
        /// <param name="parent">An optional parent container to assign the new item to.</param>
        /// <param name="json">The JSON-String to create the item from.</param>
        /// <returns>Newly created API item.</returns>
        public static IApiItem FromJson(IApiItemContainer? parent, string json) => FromJson(parent, JsonSerializer.Deserialize<JsonNode>(json));

        /// <summary>
        ///  Creates an API Item from a JSON-Node.
        /// </summary>
        /// <param name="parent">An optional parent container to assign the new item to.</param>
        /// <param name="jsonNode">The JSON-Node to create the item from.</param>
        /// <returns>Newly created API item.</returns>
        /// <exception cref="ApiJsonParsingException"></exception>
        public static IApiItem FromJson(IApiItemContainer? parent, JsonNode? jsonNode)
        {
            ArgumentNullException.ThrowIfNull(jsonNode);

            JsonValueKind valueKind = jsonNode.GetValueKind();
            switch (valueKind)
            {
                case JsonValueKind.Object:
                    JsonObject jsonObj = jsonNode.AsObject();
                    ApiItemDict dict = new ApiItemDict(parent);
                    foreach (KeyValuePair<string, JsonNode?> kv in jsonObj)
                    {
                        if (string.IsNullOrEmpty(kv.Key)) continue;
                        if (kv.Value == null) continue;

                        dict.Insert(kv.Key, FromJson(dict, kv.Value));
                    }
                    return dict;

                case JsonValueKind.Array:
                    JsonArray jsonArr = jsonNode.AsArray();
                    ApiItemList list = new ApiItemList(parent);
                    foreach (JsonNode? childNode in jsonArr)
                    {
                        if (childNode == null) continue;

                        list.Insert(FromJson(list, childNode));
                    }
                    return list;

                case JsonValueKind.String:
                    JsonValue jsonString = jsonNode.AsValue();
                    if (jsonString.TryGetValue(out DateTime dateTime))
                    {
                        return new ApiItemValue<DateTime>(parent, dateTime);
                    }
                    return new ApiItemObject<string>(parent, jsonString.GetValue<string>());

                case JsonValueKind.Number:
                    JsonValue jsonNumber = jsonNode.AsValue();
                    return new ApiItemValue<double>(parent, jsonNumber.GetValue<double>());

                case JsonValueKind.True:
                case JsonValueKind.False:
                    JsonValue jsonBoolean = jsonNode.AsValue();
                    return new ApiItemValue<bool>(parent, jsonBoolean.GetValue<bool>());

                case JsonValueKind.Null:
                    return new ApiItemObject<object>(parent, null);

                default:
                    throw new ApiJsonParsingException($"Unsupported JSON value kind: {valueKind}");
            }
        }
    }
}
