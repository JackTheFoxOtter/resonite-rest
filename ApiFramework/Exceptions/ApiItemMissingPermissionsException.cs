using ApiFramework.Enums;
using ApiFramework.Interfaces;

namespace ApiFramework.Exceptions
{
    public class ApiItemMissingPermissionsException : ApiException
    {
        public IApiItem Item { get; }
        public EditPermission MissingPermissions { get; }

        public ApiItemMissingPermissionsException(IApiItem item, EditPermission missingPermissions) : base(403)
        {
            Item = item;
            MissingPermissions = missingPermissions;
        }

        public override string ToString()
        {
            return $"{Item.ToString()} is missing the following permission(s): {MissingPermissions.ToFriendlyName()}";
        }
    }
}
