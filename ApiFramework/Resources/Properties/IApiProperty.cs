using ApiFramework.Enums;
using ApiFramework.Resources.Items;

namespace ApiFramework.Resources.Properties
{
    /// <summary>
    /// API Properties are API Items that are associated with a defined structure.
    /// As part of that defined structure, they contain the following additional information:
    /// - Where they are in the structure (API Property Path)
    /// - What Edit Permission they have
    /// </summary>
    public interface IApiProperty
    {
        public ApiPropertyPath Path { get; }
        public IApiItem Item { get; }
        public EditPermission Permission { get; }
    }
}
