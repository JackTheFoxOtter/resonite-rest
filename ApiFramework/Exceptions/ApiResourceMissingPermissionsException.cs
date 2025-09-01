using ApiFramework.Enums;
using ApiFramework.Resources;

namespace ApiFramework.Exceptions
{
    public class ApiPropertyMissingPermissionsException : ApiException
    {
        public ApiPropertyInfo PropertyInfo { get; }
        public EditPermission MissingPermissions { get; }

        public ApiPropertyMissingPermissionsException(ApiPropertyInfo propertyInfo, EditPermission missingPermissions) : base(403)
        {
            PropertyInfo = propertyInfo;
            MissingPermissions = missingPermissions;
        }

        public override string ToString()
        {
            return $"{PropertyInfo.ToString()} is missing the following permission(s): {MissingPermissions.ToFriendlyName()}";
        }
    }
}
