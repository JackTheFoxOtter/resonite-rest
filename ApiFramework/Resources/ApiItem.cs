using ApiFramework.Enums;
using ApiFramework.Exceptions;
using ApiFramework.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ApiFramework.Resources
{
    /// <summary>
    /// Represents an individually addressable "item" of data in an API.
    /// ApiItems are contained in containers and can be either editable or read-only.
    /// </summary>
    public abstract class ApiItem : IApiItem
    {
        public IApiResource? Resource { get; private set; }
        public IApiItemContainer? Parent { get; private set; }
        private EditPermission PermissionsSelf { get; }
        public EditPermission Permissions => GetPermissions();

        public ApiItem(EditPermission perms) : this(perms, null) { }
        public ApiItem(EditPermission perms, IApiItemContainer? parent)
        {
            PermissionsSelf = perms;
            if (parent != null)
            {
                SetParent(parent);
            }
        }

        public void SetParent(IApiItemContainer parent)
        {
            Resource = parent.Resource;
            Parent = parent;
        }

        private EditPermission GetPermissions()
        {
            if (Parent is IApiItem parentItem)
            {
                // Permissions can't be less restrictive than that of parent item
                return parentItem.Permissions & PermissionsSelf;
            }
            else
            {
                return PermissionsSelf;
            }
        }

        /// <summary>
        /// Tests if the current item has the necessary permissions to execute a create / modify / delete action.
        /// </summary>
        /// <param name="required">The required permissions to check for.</param>
        /// <exception cref="ApiItemMissingPermissionsException">When the current item does not have all of the required permissions.</exception>
        public void CheckPermissions(EditPermission required)
        {
            EditPermission missing = EditPermission.None;
            if (required.CanCreate() && !Permissions.CanCreate()) missing |= EditPermission.Create;
            if (required.CanModify() && !Permissions.CanModify()) missing |= EditPermission.Modify;
            if (required.CanDelete() && !Permissions.CanDelete()) missing |= EditPermission.Delete;
            if (missing > EditPermission.None) throw new ApiItemMissingPermissionsException(this, missing);
        }

        public abstract IApiItem CreateCopy(IApiItemContainer container, EditPermission perms);

        public abstract void UpdateFrom(IApiItem other, bool checkPermissions);

        public abstract JToken ToJson();

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(ToJson());
        }

        public ApiResponse ToResponse()
        {
            return new ApiResponse(200, ToJsonString());
        }

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

        public static IApiItem? FromJson(IApiItemContainer parent, string json, Dictionary<ApiPropertyPath, ApiPropertyInfo> propertyInfos) => FromJson(parent, JsonConvert.DeserializeObject<JToken>(json), propertyInfos, ApiPropertyPath.Root);
        public static IApiItem? FromJson(IApiItemContainer parent, JToken token, Dictionary<ApiPropertyPath, ApiPropertyInfo> propertyInfos) => FromJson(parent, token, propertyInfos, ApiPropertyPath.Root);
        internal static IApiItem? FromJson(IApiItemContainer? parent, JToken? token, Dictionary<ApiPropertyPath, ApiPropertyInfo>? propertyInfos, ApiPropertyPath? currentPath)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (token == null) throw new ArgumentNullException(nameof(token));
            if (propertyInfos == null) throw new ArgumentNullException(nameof(propertyInfos));
            if (currentPath == null) throw new ArgumentNullException(nameof (currentPath));

            if (!propertyInfos.ContainsKey(currentPath)) return null; // Skip any tokens that aren't defined in propertyInfos
            ApiPropertyInfo propertyInfo = propertyInfos[currentPath];

            JTokenType tokenType = token.Type;
            switch (tokenType)
            {
                case JTokenType.Object:
                    ApiItemDict dict = new ApiItemDict(propertyInfo.Permissions, parent);
                    foreach (KeyValuePair<string, JToken?> kv in (JObject) token)
                    {
                        if (kv.Value != null)
                        {
                            IApiItem? item = FromJson(dict, kv.Value, propertyInfos, currentPath.Append(kv.Key));
                            if (item != null) dict.Insert(kv.Key, item, false);
                        }
                    }
                    return dict;

                case JTokenType.Array:
                    ApiItemList list = new ApiItemList(propertyInfo.Permissions, parent);
                    for (int i = 0; i < ((JArray) token).Count; i++)
                    {
                        JToken childToken = ((JArray) token)[i];
                        IApiItem? item = FromJson(list, childToken, propertyInfos, currentPath.Append(i.ToString()));
                        if (item != null) list.Insert(item, false);
                    }
                    return list;

                case JTokenType.Date:
                    return new ApiItemValue<DateTime?>(propertyInfo.Permissions, parent, (DateTime?) ((JValue)token).Value);

                case JTokenType.String:
                    return new ApiItemValue<string?>(propertyInfo.Permissions, parent, (string?) ((JValue) token).Value);

                case JTokenType.Integer:
                    return new ApiItemValue<long?>(propertyInfo.Permissions, parent, (long?) ((JValue) token).Value);

                case JTokenType.Float:
                    return new ApiItemValue<float?>(propertyInfo.Permissions, parent, (float?)((JValue) token).Value);

                case JTokenType.Boolean:
                    return new ApiItemValue<bool?>(propertyInfo.Permissions, parent, (bool?)((JValue) token).Value);

                case JTokenType.Null:
                    return new ApiItemValue<object?>(propertyInfo.Permissions, parent, null);

                default:
                    throw new ApiJsonParsingException($"Unsupported token type: {tokenType}");
            }
        }

        public static IApiItem CreateNewForProperty(IApiItemContainer parent, ApiPropertyInfo propertyInfo)
        {
            if (typeof(ApiItemDict).IsAssignableFrom(propertyInfo.TargetType))
            {
                // New ApiItemDict
                return new ApiItemDict(propertyInfo.Permissions, parent);
            }
            else if (typeof(ApiItemList).IsAssignableFrom(propertyInfo.TargetType))
            {
                // New ApiItemList
                return new ApiItemList(propertyInfo.Permissions, parent);
            }
            else if (propertyInfo.TargetType.IsGenericType && propertyInfo.TargetType.GetGenericTypeDefinition() == typeof(ApiItemValue<>))
            {
                // New ApiItemValue
                Type valueType = propertyInfo.TargetType.GenericTypeArguments[0];
                if (valueType.IsValueType)
                {
                    return (IApiItem)Activator.CreateInstance(propertyInfo.TargetType, propertyInfo.Permissions, parent, Activator.CreateInstance(valueType));
                }
                else
                {
                    return (IApiItem)Activator.CreateInstance(propertyInfo.TargetType, propertyInfo.Permissions, parent, null);
                }
            }
            else
            {
                // Can't create instance
                throw new ArgumentException($"Can't create new ApiItem instance for type {propertyInfo.TargetType}!");
            }
        }
    }
}
