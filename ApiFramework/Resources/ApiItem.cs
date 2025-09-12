using ApiFramework.Exceptions;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ApiFramework.Resources
{
    /// <summary>
    /// Represents an individually addressable "item" of data in an API.
    /// ApiItems are contained in containers and can be either editable or read-only.
    /// </summary>
    public abstract class ApiItem : IApiItem
    {
        public IApiItemContainer? Parent { get; private set; }

        public void SetParent(IApiItemContainer? newParent)
        {
            if (Parent == newParent) return;

            Parent?.RemoveItem(this);
            Parent = newParent;
        }

        public ApiItem() { }

        public ApiItem(IApiItemContainer? parent) { SetParent(parent); }

        public abstract void UpdateFrom(IApiItem other);

        public abstract IApiItem CreateCopy();

        public abstract JsonNode ToJson();

        public override string ToString()
        {
            if (Parent != null && Parent.Contains(this))
            {
                if (Parent is ApiItem parentItem)
                {
                    return $"{Parent.ToString()}.{Parent.NameOf(this)}";
                }
                else
                {
                    return Parent.NameOf(this);
                }
            }

            return $"<Unbound: {GetType().GetNiceTypeName()}>";
        }

        public static IApiItem FromJson(IApiItemContainer? parent, string json) => FromJson(parent, JsonSerializer.Deserialize<JsonNode>(json));

        public static IApiItem FromJson(IApiItemContainer? parent, JsonNode jsonNode)
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

        // TODO: This belongs into Property
        //public static T CreateNewForProperty<T>(IApiItemContainer? parent, ApiPropertyInfo propertyInfo, bool checkPermissions) where T : ApiItem
        //{
        //    if (checkPermissions) propertyInfo.CheckPermissions(EditPermission.Create);
        //    if (!typeof(T).IsAssignableFrom(propertyInfo.TargetType)) 
        //        throw new ArgumentException($"Property type {propertyInfo.TargetType.GetNiceTypeName()} is incompatible with generic argument type {typeof(T).GetNiceTypeName()}!");

        //    if (typeof(ApiItemDict).IsAssignableFrom(propertyInfo.TargetType) || typeof(ApiItemList).IsAssignableFrom(propertyInfo.TargetType))
        //    {
        //        // New ApiItemDict or ApiItemList
        //        return (T)Activator.CreateInstance(propertyInfo.TargetType, propertyInfo.Permissions, parent);
        //    }
        //    else if (propertyInfo.TargetType.IsGenericType && propertyInfo.TargetType.GetGenericTypeDefinition() == typeof(ApiItemValue<>))
        //    {
        //        // New ApiItemValue
        //        Type valueType = propertyInfo.TargetType.GenericTypeArguments[0];
        //        if (valueType.IsValueType)
        //        {
        //            return (T)Activator.CreateInstance(propertyInfo.TargetType, propertyInfo.Permissions, parent, Activator.CreateInstance(valueType));
        //        }
        //        else
        //        {
        //            return (T)Activator.CreateInstance(propertyInfo.TargetType, propertyInfo.Permissions, parent, null);
        //        }
        //    }
        //    else
        //    {
        //        // Can't create instance
        //        throw new ArgumentException($"Can't create new ApiItem instance for type {propertyInfo.TargetType}!");
        //    }
        //}
    }
}
