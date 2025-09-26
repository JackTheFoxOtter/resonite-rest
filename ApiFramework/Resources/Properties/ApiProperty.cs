using ApiFramework.Enums;
using ApiFramework.Exceptions;
using ApiFramework.Resources.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ApiFramework.Resources.Properties
{
    public class ApiProperty<T> : IApiProperty<T>, IEqualityComparer<IApiProperty<T>>, IComparable<IApiProperty<T>> where T : IApiItem
    {
        public IApiResource Resource { get; }
        public T Item { get; }
        public ApiPropertyPath Path { get; }
        public EditPermission Permission { get; }

        public ApiProperty(string fullPath, T item, EditPermission perms) : this(ApiPropertyPath.FromFullPath(fullPath), item, perms) { }
        public ApiProperty(ApiPropertyPath targetPath, T item, EditPermission perms)
        {
            Path = targetPath;
            Item = item;
            Permission = perms;
        }

        /// <summary>
        /// Tests if the current item has the necessary permissions to execute a create / modify / delete action.
        /// </summary>
        /// <param name="required">The required permissions to check for.</param>
        /// <exception cref="ApiResourceMissingPermissionsException">When the current item does not have all of the required permissions.</exception>
        public void CheckPermissions(EditPermission required)
        {
            EditPermission missing = EditPermission.None;
            if (required.CanCreate() && !Permission.CanCreate()) missing |= EditPermission.Create;
            if (required.CanModify() && !Permission.CanModify()) missing |= EditPermission.Modify;
            if (required.CanDelete() && !Permission.CanDelete()) missing |= EditPermission.Delete;
            if (missing > EditPermission.None) throw new ApiPropertyMissingPermissionsException(this, missing);
        }

        public int CompareTo(IApiProperty<T>? other)
        {
            if (other == null) return -1;
            return Path.CompareTo(other.Path);
        }

        public bool Equals(IApiProperty<T>? x, IApiProperty<T>? y)
        {
            if (x == null || y == null) return x == y;
            return x.Path.Equals(y.Path);
        }

        public int GetHashCode(IApiProperty<T> obj)
        {
            ArgumentNullException.ThrowIfNull(obj);
            return Path.GetHashCode();
        }

        public IApiProperty<T> CreateCopy()
        {
            throw new NotImplementedException();
        }

        public JsonNode ToJson()
        {
            return Item.ToJson();
        }

        public static IApiProperty FromJson(IApiItemContainer? parent, ApiPropertyPath path, JsonNode? jsonNode)
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
