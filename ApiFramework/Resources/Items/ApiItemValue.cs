using ApiFramework.Exceptions;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ApiFramework.Resources.Items
{
    /// <summary>
    /// API Item to represent a value type item.
    /// For reference / object types, use ApiItemObject.
    /// </summary>
    /// <typeparam name="T">Item Type</typeparam>
    public class ApiItemValue<T> : ApiItem where T : struct
    {
        private T? _value;

        /// <summary>
        /// The value represented by this item.
        /// </summary>
        public T? Value
        {
            get => _value;
            set => _value = value;
        }

        /// <summary>
        /// Creates a new ApiItemValue instance.
        /// </summary>
        public ApiItemValue() : base() { }

        /// <summary>
        /// Creates a new ApiItemValue instance.
        /// </summary>
        /// <param name="parent">The container to assign the item to.</param>
        public ApiItemValue(IApiItemContainer? parent) : base(parent) { }

        /// <summary>
        /// Creates a new ApiItemValue instance.
        /// </summary>
        /// <param name="parent">The container to assign the item to.</param>
        /// <param name="value">The initial value of the item.</param>
        public ApiItemValue(IApiItemContainer? parent, T? value) : base(parent) { _value = value; }

        public override JsonNode ToJson()
        {
            return JsonSerializer.SerializeToNode(Value) ?? throw new ApiJsonParsingException("Failed to serialize ApiItemValue to JSON-Node!");
        }

        public override void UpdateFrom(IApiItem other)
        {
            ArgumentNullException.ThrowIfNull(other);
            if (other is not ApiItemValue<T> otherItem) throw new ArgumentException($"Can't update {GetType()} from item of different type {other.GetType()}");

            // Value type -> pass by value
            Value = otherItem.Value;
        }

        public override IApiItem CreateCopy()
        {
            ApiItemValue<T> copy = new();
            // Value type -> pass by value
            copy.Value = Value;
            return copy;
        }
    }
}
