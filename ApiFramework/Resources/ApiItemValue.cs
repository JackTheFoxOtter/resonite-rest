using ApiFramework.Enums;
using ApiFramework.Exceptions;
using ApiFramework.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace ApiFramework.Resources
{
    public class ApiItemValue<T> : ApiItem
    {
        private T? _value;
        public T? Value
        {
            get => _value;
            set
            {
                if (!CanModify) throw new ApiResourceItemReadOnlyException(ToString());
                _value = value;
            }
        }

        public ApiItemValue(IApiItemContainer parent, EditPermission perms, T? value) : base(parent, perms)
        {
            _value = value;
        }

        public override JToken ToJson()
        {
            return new JValue(Value);
        }

        public override IApiItem CreateCopy(IApiItemContainer container, EditPermission perms)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            if (typeof(T).IsValueType)
            {
                // Value type -> pass by value
                return new ApiItemValue<T>(container, perms, Value);
            }
            else
            {
                // Reference type -> pass by reference, so create copy through serializing / deseraializing
                return new ApiItemValue<T>(container, perms, JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(Value)));
            }
        }

        public override void UpdateFrom(IApiItem other)
        {
            if (!CanModify) throw new ApiResourceItemReadOnlyException(ToString());
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (other is not ApiItemValue<T> otherValue) throw new ArgumentException($"Can't update {GetType()} from item of different type {other.GetType()}");

            if (typeof(T).IsValueType)
            {
                // Value type -> pass by value
                Value = otherValue.Value;
            }
            else
            {
                // Reference type -> pass by reference, so create copy through serializing / deseraializing
                Value = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(otherValue.Value));
            }
        }
    }
}
