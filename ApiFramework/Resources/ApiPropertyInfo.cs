using ApiFramework.Enums;
using ApiFramework.Exceptions;
using ApiFramework.Interfaces;
using System;
using System.Collections.Generic;

namespace ApiFramework.Resources
{
    public class ApiPropertyInfo : IEqualityComparer<ApiPropertyInfo>, IComparable<ApiPropertyInfo>
    {
        public ApiResource Resource { get; }
        public ApiPropertyPath Path { get; }
        public Type TargetType { get; }
        public EditPermission Permissions { get; }

        public ApiPropertyInfo(ApiResource resource, string fullPath, Type targetType, EditPermission perms) : this(resource, ApiPropertyPath.FromFullPath(fullPath), targetType, perms) { }
        public ApiPropertyInfo(ApiResource resource, ApiPropertyPath targetPath, Type targetType, EditPermission perms)
        {
            if (!typeof(IApiItem).IsAssignableFrom(targetType))
                throw new ApiInvalidItemTypeException(resource.GetType(), targetType);

            Resource = resource;
            Path = targetPath;
            TargetType = targetType;
            Permissions = perms;
        }

        /// <summary>
        /// Tests if the current item has the necessary permissions to execute a create / modify / delete action.
        /// </summary>
        /// <param name="required">The required permissions to check for.</param>
        /// <exception cref="ApiResourceMissingPermissionsException">When the current item does not have all of the required permissions.</exception>
        public void CheckPermissions(EditPermission required)
        {
            EditPermission missing = EditPermission.None;
            if (required.CanCreate() && !Permissions.CanCreate()) missing |= EditPermission.Create;
            if (required.CanModify() && !Permissions.CanModify()) missing |= EditPermission.Modify;
            if (required.CanDelete() && !Permissions.CanDelete()) missing |= EditPermission.Delete;
            if (missing > EditPermission.None) throw new ApiPropertyMissingPermissionsException(this, missing);
        }

        public int CompareTo(ApiPropertyInfo other)
        {
            return Path.CompareTo(other.Path);
        }

        public bool Equals(ApiPropertyInfo x, ApiPropertyInfo y)
        {
            return x.Path.Equals(y.Path);
        }

        public int GetHashCode(ApiPropertyInfo obj)
        {
            return Path.GetHashCode();
        }
    }
}
