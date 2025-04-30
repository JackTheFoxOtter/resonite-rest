using ApiFramework.Exceptions;
using ApiFramework.Interfaces;
using Elements.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiFramework.Resources
{
    public abstract class ApiItem : IApiItem
    {
        private readonly IApiItemContainer _parent;
        private readonly bool _canEdit;

        public ApiItem(IApiItemContainer parent, bool canEdit)
        {
            _parent = parent;
            _canEdit = _parent.CanEdit() && canEdit;
        }

        public IApiItemContainer GetParent()
        {
            return _parent;
        }

        public bool CanEdit()
        {
            return _canEdit;
        }

        public abstract JToken ToJsonRepresentation();

        public abstract IApiItem CreateCopy(IApiItemContainer container, bool canEdit);

        public abstract void UpdateFrom(IApiItem other);

        public string ToJson()
        {
            return JsonConvert.SerializeObject(ToJsonRepresentation());
        }

        public ApiResponse ToResponse()
        {
            return new ApiResponse(200, JsonConvert.SerializeObject(ToJsonRepresentation()));
        }

        public override string ToString()
        {
            return GetParent().NameOf(this);
        }

        public static IApiItem FromJson(IApiItemContainer parent, Func<string[], bool> canEditCheck, string json)
        {
            JToken? rootToken = (JToken?) JsonConvert.DeserializeObject(json);
            if (rootToken == null) throw new ApiJsonParsingException("Deserialized JSON object is null");
            
            return FromJson(parent, canEditCheck, rootToken);
        }

        public static IApiItem FromJson(IApiItemContainer parent, Func<string[], bool> canEditCheck, JToken token)
        {
            return FromJson(parent, canEditCheck, token, new string[]{ });
        }

        internal static IApiItem FromJson(IApiItemContainer parent, Func<string[], bool> canEditCheck, JToken token, string[] itemPath)
        {
            JTokenType tokenType = token.Type;
            switch (tokenType)
            {
                case JTokenType.Object:
                    ApiItemDict dict = new ApiItemDict(parent, canEditCheck(itemPath));
                    foreach (KeyValuePair<string, JToken?> kv in (JObject) token)
                    {
                        if (kv.Value != null)
                        {
                            dict.Insert(kv.Key, FromJson(dict, canEditCheck, kv.Value, itemPath.Append(kv.Key).ToArray()), false);
                        }
                    }
                    return dict;

                case JTokenType.Array:
                    ApiItemList list = new ApiItemList(parent, canEditCheck(itemPath));
                    for (int i = 0; i < ((JArray) token).Count; i++)
                    {
                        JToken childToken = ((JArray) token)[i];
                        list.Insert(FromJson(list, canEditCheck, childToken, itemPath.Append(i.ToString()).ToArray()), false);
                    }
                    return list;

                case JTokenType.Date:
                    return new ApiItemValue<DateTime?>(parent, canEditCheck(itemPath), (DateTime?) ((JValue)token).Value);

                case JTokenType.String:
                    return new ApiItemValue<string?>(parent, canEditCheck(itemPath), (string?) ((JValue) token).Value);

                case JTokenType.Integer:
                    return new ApiItemValue<long?>(parent, canEditCheck(itemPath), (long?) ((JValue) token).Value);

                case JTokenType.Float:
                    return new ApiItemValue<float?>(parent, canEditCheck(itemPath), (float?)((JValue) token).Value);

                case JTokenType.Boolean:
                    return new ApiItemValue<bool?>(parent, canEditCheck(itemPath), (bool?)((JValue) token).Value);

                case JTokenType.Null:
                    return new ApiItemValue<object?>(parent, canEditCheck(itemPath), null);

                default:
                    throw new ApiJsonParsingException($"Unsupported token type: {tokenType}");
            }
        }
    }
}
