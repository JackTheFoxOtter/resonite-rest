using ApiFramework.Enums;
using ApiFramework.Exceptions;
using ApiFramework.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiFramework.Resources
{
    /// <summary>
    /// Represents an individually addressable "item" of data in an API.
    /// ApiItems are contained in containers and can be either editable or read-only.
    /// </summary>
    public abstract class ApiItem : IApiItem
    {
        public IApiItemContainer Parent { get; }
        public EditPermission Permissions { get; }
        public bool CanCreate => (Permissions | EditPermission.Create) > 0; // TODO: Move into IApiItem once on .Net9
        public bool CanModify => (Permissions | EditPermission.Modify) > 0; // TODO: Move into IApiItem once on .Net9
        public bool CanDelete => (Permissions | EditPermission.Delete) > 0; // TODO: Move into IApiItem once on .Net9

        public ApiItem(IApiItemContainer parent, EditPermission perms)
        {
            Parent = parent;
            Permissions = (Parent is IApiItem parentItem) ? parentItem.Permissions & perms : Permissions = perms;
        }

        public abstract IApiItem CreateCopy(IApiItemContainer container, EditPermission perms);

        public abstract void UpdateFrom(IApiItem other);

        public abstract JToken ToJson();

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(ToJson());
        }

        public ApiResponse ToResponse()
        {
            return new ApiResponse(200, JsonConvert.SerializeObject(ToJson()));
        }

        public override string ToString()
        {
            return Parent.NameOf(this);
        }

        public static IApiItem FromJson(IApiItemContainer parent, Func<string[], EditPermission> getPerms, string json)
        {
            JToken? rootToken = (JToken?) JsonConvert.DeserializeObject(json);
            if (rootToken == null) throw new ApiJsonParsingException("Deserialized JSON object is null");
            
            return FromJson(parent, getPerms, rootToken);
        }

        public static IApiItem FromJson(IApiItemContainer parent, Func<string[], EditPermission> getPerms, JToken token)
        {
            return FromJson(parent, getPerms, token, new string[]{ });
        }

        internal static IApiItem FromJson(IApiItemContainer parent, Func<string[], EditPermission> getPerms, JToken token, string[] itemPath)
        {
            JTokenType tokenType = token.Type;
            switch (tokenType)
            {
                case JTokenType.Object:
                    ApiItemDict dict = new ApiItemDict(parent, getPerms(itemPath));
                    foreach (KeyValuePair<string, JToken?> kv in (JObject) token)
                    {
                        if (kv.Value != null)
                        {
                            dict.Insert(kv.Key, FromJson(dict, getPerms, kv.Value, itemPath.Append(kv.Key).ToArray()), false);
                        }
                    }
                    return dict;

                case JTokenType.Array:
                    ApiItemList list = new ApiItemList(parent, getPerms(itemPath));
                    for (int i = 0; i < ((JArray) token).Count; i++)
                    {
                        JToken childToken = ((JArray) token)[i];
                        list.Insert(FromJson(list, getPerms, childToken, itemPath.Append(i.ToString()).ToArray()), false);
                    }
                    return list;

                case JTokenType.Date:
                    return new ApiItemValue<DateTime?>(parent, getPerms(itemPath), (DateTime?) ((JValue)token).Value);

                case JTokenType.String:
                    return new ApiItemValue<string?>(parent, getPerms(itemPath), (string?) ((JValue) token).Value);

                case JTokenType.Integer:
                    return new ApiItemValue<long?>(parent, getPerms(itemPath), (long?) ((JValue) token).Value);

                case JTokenType.Float:
                    return new ApiItemValue<float?>(parent, getPerms(itemPath), (float?)((JValue) token).Value);

                case JTokenType.Boolean:
                    return new ApiItemValue<bool?>(parent, getPerms(itemPath), (bool?)((JValue) token).Value);

                case JTokenType.Null:
                    return new ApiItemValue<object?>(parent, getPerms(itemPath), null);

                default:
                    throw new ApiJsonParsingException($"Unsupported token type: {tokenType}");
            }
        }
    }
}
