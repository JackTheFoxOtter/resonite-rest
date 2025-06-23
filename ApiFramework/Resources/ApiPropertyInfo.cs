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
