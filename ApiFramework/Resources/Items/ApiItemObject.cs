using ApiFramework.Exceptions;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ApiFramework.Resources.Items
{
    /// <summary>
    /// API Item to represent an object / reference type item.
    /// For value types, use ApiItemValue.
    /// </summary>
    /// <typeparam name="T">Item Type</typeparam>
    public class ApiItemObject<T> : ApiItem where T : class
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
        /// Creates a new ApiItemObject instance.
        /// </summary>
        public ApiItemObject() : base() { }

        /// <summary>
        /// Creates a new ApiItemObject instance.
        /// </summary>
        /// <param name="parent">The container to assign the item to.</param>
        public ApiItemObject(IApiItemContainer? parent) : base(parent) { }

        /// <summary>
        /// Creates a new ApiItemObject instance. 
        /// </summary>
        /// <param name="parent">The container to assign the item to.</param>
        /// <param name="value">The initial value of the item.</param>
        public ApiItemObject(IApiItemContainer? parent, T? value) : base(parent) { _value = value; }

        public override JsonNode ToJson()
        {
            return JsonSerializer.SerializeToNode(Value) ?? throw new ApiJsonParsingException("Failed to serialize ApiItemObject to JSON-Node!");
        }

        public override void UpdateFrom(IApiItem other)
        {
            ArgumentNullException.ThrowIfNull(other);
            if (other is not ApiItemObject<T> otherItem) throw new ArgumentException($"Can't update {GetType()} from item of different type {other.GetType()}");

            // Reference type -> pass by reference, so create copy through serializing / deserializing
            Value = JsonSerializer.SerializeToNode(otherItem.Value).Deserialize<T?>();
        }

        public override IApiItem CreateCopy()
        {
            ApiItemObject<T> copy = new();
            // Reference type -> pass by reference, so create copy through serializing / deserializing
            copy.Value = JsonSerializer.SerializeToNode(Value).Deserialize<T?>();
            return copy;
        }
    }
}
