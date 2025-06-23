using ApiFramework.Enums;
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
                CheckPermissions(EditPermission.Modify);
                _value = value;
            }
        }

        public ApiItemValue(EditPermission perms, T? value) : base(perms) { }
        public ApiItemValue(EditPermission perms, IApiItemContainer parent, T? value) : base(perms, parent)
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
                return new ApiItemValue<T>(perms, container, Value);
            }
            else
            {
                // Reference type -> pass by reference, so create copy through serializing / deseraializing
                return new ApiItemValue<T>(perms, container, JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(Value)));
            }
        }

        public override void UpdateFrom(IApiItem other, bool checkPermissions)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (other is not ApiItemValue<T> otherValue) throw new ArgumentException($"Can't update {GetType()} from item of different type {other.GetType()}");
            if (checkPermissions) CheckPermissions(EditPermission.Modify); 

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
