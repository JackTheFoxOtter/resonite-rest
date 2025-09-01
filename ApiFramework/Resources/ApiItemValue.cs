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
            set => SetValue(value, true);
        }

        public ApiItemValue(ApiPropertyInfo propertyInfo, IApiItemContainer parent) : base(propertyInfo, parent) { }

        public void SetValue(T? newValue, bool checkPermissions)
        {
            if(checkPermissions) PropertyInfo.CheckPermissions(EditPermission.Modify);
            _value = newValue;
        }

        public override JToken ToJson()
        {
            return new JValue(Value);
        }

        public override IApiItem CopyTo(ApiPropertyInfo newPropertyInfo, IApiItemContainer newParent, bool checkPermissions)
        {
            if (newParent == null) throw new ArgumentNullException(nameof(newParent));
            if (newPropertyInfo == null) throw new ArgumentNullException(nameof(newPropertyInfo));

            ApiItemValue<T> newItem = new(newPropertyInfo, newParent);
            if (typeof(T).IsValueType)
            {
                // Value type -> pass by value
                newItem.SetValue(Value, checkPermissions);
            }
            else
            {
                // Reference type -> pass by reference, so create copy through serializing / deseraializing
                newItem.SetValue(JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(Value)), checkPermissions);
            }
            return newItem;
        }

        public override void UpdateFrom(IApiItem other, bool checkPermissions)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (other is not ApiItemValue<T> otherValue) throw new ArgumentException($"Can't update {GetType()} from item of different type {other.GetType()}");
            if (checkPermissions) PropertyInfo.CheckPermissions(EditPermission.Modify); 

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
