using ApiFramework.Enums;
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
        public ApiPropertyInfo PropertyInfo { get; private set; }
        public IApiItemContainer Parent { get; private set; }

        public ApiItem(ApiPropertyInfo propertyInfo, IApiItemContainer parent)
        {
            PropertyInfo = propertyInfo;
            Parent = parent;
        }

        public IApiItem CopyTo(ApiPropertyInfo newPropertyInfo, IApiItemContainer newParent) => CopyTo(newPropertyInfo, newParent, true);

        public abstract IApiItem CopyTo(ApiPropertyInfo newPropertyInfo, IApiItemContainer newParent, bool checkPermissions);

        public void UpdateFrom(IApiItem other) => UpdateFrom(other, true);

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

        public static IApiItem? FromJson(ApiPropertyInfo propertyInfo, IApiItemContainer parent, string json) => FromJson(propertyInfo, parent, JsonConvert.DeserializeObject<JToken>(json), ApiPropertyPath.Root);
        public static IApiItem? FromJson(ApiPropertyInfo propertyInfo, IApiItemContainer parent, JToken token) => FromJson(propertyInfo, parent, token, ApiPropertyPath.Root);
        internal static IApiItem? FromJson(ApiPropertyInfo propertyInfo, IApiItemContainer parent, JToken token, ApiPropertyPath currentPath)
        {
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (token == null) throw new ArgumentNullException(nameof(token));
            if (currentPath == null) throw new ArgumentNullException(nameof(currentPath));

            //if (!propertyInfos.ContainsKey(currentPath)) return null; // Skip any tokens that aren't defined in propertyInfos
            //ApiPropertyInfo propertyInfo = propertyInfos[currentPath];

            JTokenType tokenType = token.Type;
            switch (tokenType)
            {
                case JTokenType.Object:
                    ApiItemDict dict = new ApiItemDict(propertyInfo, parent);
                    foreach (KeyValuePair<string, JToken?> kv in (JObject)token)
                    {
                        if (kv.Value != null)
                        {
                            ApiPropertyInfo itemPropertyInfo = propertyInfo.Resource.PropertyInfos[propertyInfo.Path.Append(kv.Key)];
                            IApiItem newItem = dict.InsertNew<IApiItem>(kv.Key, itemPropertyInfo, false);

                            IApiItem? item = FromJson(dict, kv.Value, propertyInfos, currentPath.Append(kv.Key));
                        }
                    }
                    return dict;

                case JTokenType.Array:
                    ApiItemList list = new ApiItemList(propertyInfo, parent);
                    for (int i = 0; i < ((JArray)token).Count; i++)
                    {
                        JToken childToken = ((JArray)token)[i];
                        IApiItem? item = FromJson(list, childToken, propertyInfos, currentPath.Append(i.ToString()));
                        if (item != null) list.Insert(item, false);
                    }
                    return list;

                case JTokenType.Date:
                    ApiItemValue<DateTime?> newDateItem = new ApiItemValue<DateTime?>(propertyInfo, parent);
                    newDateItem.SetValue()
                    return (DateTime?)((JValue)token).Value);

                case JTokenType.String:
                    return new ApiItemValue<string?>(propertyInfo, parent, (string?)((JValue)token).Value);

                case JTokenType.Integer:
                    return new ApiItemValue<long?>(propertyInfo, parent, (long?)((JValue)token).Value);

                case JTokenType.Float:
                    return new ApiItemValue<float?>(propertyInfo, parent, (float?)((JValue)token).Value);

                case JTokenType.Boolean:
                    return new ApiItemValue<bool?>(propertyInfo, parent, (bool?)((JValue)token).Value);

                case JTokenType.Null:
                    return new ApiItemValue<object?>(propertyInfo, parent, null);

                default:
                    throw new ApiJsonParsingException($"Unsupported token type: {tokenType}");
            }
        }

        public static T CreateNewForProperty<T>(IApiItemContainer? parent, ApiPropertyInfo propertyInfo, bool checkPermissions) where T : ApiItem
        {
            if (checkPermissions) propertyInfo.CheckPermissions(EditPermission.Create);
            if (!typeof(T).IsAssignableFrom(propertyInfo.TargetType)) 
                throw new ArgumentException($"Property type {propertyInfo.TargetType.GetNiceTypeName()} is incompatible with generic argument type {typeof(T).GetNiceTypeName()}!");

            if (typeof(ApiItemDict).IsAssignableFrom(propertyInfo.TargetType) || typeof(ApiItemList).IsAssignableFrom(propertyInfo.TargetType))
            {
                // New ApiItemDict or ApiItemList
                return (T)Activator.CreateInstance(propertyInfo.TargetType, propertyInfo.Permissions, parent);
            }
            else if (propertyInfo.TargetType.IsGenericType && propertyInfo.TargetType.GetGenericTypeDefinition() == typeof(ApiItemValue<>))
            {
                // New ApiItemValue
                Type valueType = propertyInfo.TargetType.GenericTypeArguments[0];
                if (valueType.IsValueType)
                {
                    return (T)Activator.CreateInstance(propertyInfo.TargetType, propertyInfo.Permissions, parent, Activator.CreateInstance(valueType));
                }
                else
                {
                    return (T)Activator.CreateInstance(propertyInfo.TargetType, propertyInfo.Permissions, parent, null);
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
