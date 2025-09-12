using ApiFramework.Exceptions;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ApiFramework.Resources
{
    public class ApiItemObject<T> : ApiItem where T : class
    {
        private T? _value;
        public T? Value
        {
            get => _value;
            set => _value = value;
        }

        public ApiItemObject() : base() { }

        public ApiItemObject(IApiItemContainer? parent) : base(parent) { }

        public ApiItemObject(IApiItemContainer? parent, T? value) : base(parent) { _value = value; }

        public override JsonNode ToJson()
        {
            return JsonSerializer.SerializeToNode(Value) ?? throw new ApiJsonParsingException("Failed to serialize ApiItemObject to JSON-Node!");
        }

        public override void UpdateFrom(IApiItem other)
        {
            ArgumentNullException.ThrowIfNull(other);
            if (other is not ApiItemObject<T> otherItem) throw new ArgumentException($"Can't update {GetType()} from item of different type {other.GetType()}");

            // Reference type -> pass by reference, so create copy through serializing / deseraializing
            Value = JsonSerializer.SerializeToNode(otherItem.Value).Deserialize<T?>();
        }

        public override IApiItem CreateCopy()
        {
            ApiItemObject<T> copy = new();
            // Reference type -> pass by reference, so create copy through serializing / deseraializing
            copy.Value = JsonSerializer.SerializeToNode(Value).Deserialize<T?>();
            return copy;
        }
    }
}
