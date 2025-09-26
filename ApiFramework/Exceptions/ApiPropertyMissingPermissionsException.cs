using ApiFramework.Enums;
using ApiFramework.Resources;
using ApiFramework.Resources.Properties;

namespace ApiFramework.Exceptions
{
    public class ApiPropertyMissingPermissionsException : ApiException
    {
        public ApiProperty Property { get; }
        public EditPermission MissingPermissions { get; }

        public ApiPropertyMissingPermissionsException(ApiProperty property, EditPermission missingPermissions) : base(403)
        {
            Property = property;
            MissingPermissions = missingPermissions;
        }

        public override string ToString()
        {
            return $"{Property.ToString()} is missing the following permission(s): {MissingPermissions.ToFriendlyName()}";
        }
    }
}
