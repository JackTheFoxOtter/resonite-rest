using ApiFramework.Enums;
using ApiFramework.Resources.Items;
using System;
using System.Collections.Generic;

namespace ApiFramework.Resources.Properties
{
    internal class ApiProperty : IApiProperty, IEqualityComparer<ApiProperty>, IComparable<ApiProperty>
    {
        public ApiPropertyPath Path { get; }
        public IApiItem Item { get; }
        public EditPermission Permission { get; }

        public ApiProperty(string fullPath, IApiItem item, EditPermission perms) : this(ApiPropertyPath.FromFullPath(fullPath), item, perms) { }
        public ApiProperty(ApiPropertyPath targetPath, IApiItem item, EditPermission perms)
        {
            Path = targetPath;
            Item = item;
            Permission = perms;
        }

        /// <summary>
        /// Tests if the current item has the necessary permissions to execute a create / modify / delete action.
        /// </summary>
        /// <param name="required">The required permissions to check for.</param>
        /// <exception cref="ApiResourceMissingPermissionsException">When the current item does not have all of the required permissions.</exception>
        public void CheckPermissions(EditPermission required)
        {
            EditPermission missing = EditPermission.None;
            if (required.CanCreate() && !Permission.CanCreate()) missing |= EditPermission.Create;
            if (required.CanModify() && !Permission.CanModify()) missing |= EditPermission.Modify;
            if (required.CanDelete() && !Permission.CanDelete()) missing |= EditPermission.Delete;
            if (missing > EditPermission.None) throw new ApiPropertyMissingPermissionsException(this, missing);

            // actions/setup.dotnet
            // actions/
        }

        public int CompareTo(ApiProperty other)
        {
            return Path.CompareTo(other.Path);
        }

        public bool Equals(ApiProperty x, ApiProperty y)
        {
            return x.Path.Equals(y.Path);
        }

        public int GetHashCode(ApiProperty obj)
        {
            return Path.GetHashCode();
        }
    }
}
