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

        public ApiItemValue(IApiItemContainer parent, bool canEdit, T? value) : base(parent, canEdit)
        {
            _value = value;
        }

        public T? Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (!CanEdit()) throw new ApiResourceItemReadOnlyException(ToString());
                _value = value;
            }
        }

        public override JToken ToJsonRepresentation()
        {
            return new JValue(Value);
        }

        public override IApiItem CreateCopy(IApiItemContainer container, bool canEdit)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (typeof(T).IsValueType)
            {
                // Value type -> pass by value
                return new ApiItemValue<T>(container, canEdit, Value);
            }
            else
            {
                // Reference type -> pass by reference, so create copy through serializing / deseraializing
                return new ApiItemValue<T>(container, canEdit, JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(Value)));
            }
        }

        public override void UpdateFrom(IApiItem other)
        {
            if (!CanEdit()) throw new ApiResourceItemReadOnlyException(ToString());
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (other is ApiItemValue<T> otherValue)
            {
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
            else
            {
                throw new ArgumentException($"Can't update {GetType()} from item of different type {other.GetType()}");
            }
        }
    }
}
