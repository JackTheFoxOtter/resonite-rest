﻿using ApiFramework.Enums;
using ApiFramework.Exceptions;
using ApiFramework.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ApiFramework.Resources
{
    public abstract class ApiResource : IApiResource, IApiItemContainer
    {
        private static Dictionary<Type, Dictionary<ApiPropertyPath, ApiPropertyInfo>> _propertyInfosCache = new();
        public Dictionary<ApiPropertyPath, ApiPropertyInfo> PropertyInfos => GetOrInitializePropertyInfos();
        public string ResourceName => GetType().Name;
        public IApiResource Resource => this;
        public IApiItem RootItem { get; }

        public ApiResource() : this(null, null) { }
        public ApiResource(string json) : this(JsonConvert.DeserializeObject<JToken>(json)) { }
        public ApiResource(JToken? json) : this(null, json) { }
        public ApiResource(IApiItem? rootItem, JToken? json)
        {
            if (json != null)
            {
                RootItem = ApiItem.FromJson(this, json, PropertyInfos) ?? throw new ApiJsonParsingException($"Null item after parsing JSON to create {GetType().Name}.");
            }
            else
            {
                RootItem = rootItem ?? new ApiItemDict(EditPermission.CreateModifyDelete, this);
            }
        }

        private Dictionary<ApiPropertyPath, ApiPropertyInfo> GetOrInitializePropertyInfos()
        {
            if (_propertyInfosCache.ContainsKey(GetType()))
                return _propertyInfosCache[GetType()];

            // First call, initialize property infos for resource type
            Dictionary<ApiPropertyPath, ApiPropertyInfo> propertyInfos = new()
            {
                { ApiPropertyPath.Root, new ApiPropertyInfo(this, ApiPropertyPath.Root, typeof(ApiItemDict), Enums.EditPermission.CreateModifyDelete) } // Implicit root property
            };

            List<ApiPropertyInfo> propertyInfoList = new(GetPropertyInfos());
            propertyInfoList.Sort(); // Sorts by length of segments first)
            foreach (ApiPropertyInfo propertyInfo in propertyInfoList)
            {
                if (propertyInfo.Path == ApiPropertyPath.Root) continue; // Can't override implicit root
                propertyInfos[propertyInfo.Path] = propertyInfo;
            }

            _propertyInfosCache.Add(GetType(), propertyInfos);
            return propertyInfos;
        }

        protected abstract ApiPropertyInfo[] GetPropertyInfos();

        public void UpdateFrom(ApiResource other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (GetType() != other.GetType()) throw new ArgumentException($"Can't update {GetType()} from resource with different type ({other.GetType()})");

            throw new NotImplementedException();
        }

        public int Count()
        {
            return 1;
        }

        public bool Contains(IApiItem item)
        {
            return item == RootItem;
        }

        public string NameOf(IApiItem item)
        {
            if (item != RootItem) throw new ArgumentException($"ApiResource doesn't contain item {item}");

            return ResourceName;
        }

        public T? GetProperty<T>(string fullPath) where T : IApiItem => GetProperty<T>(ApiPropertyPath.FromFullPath(fullPath));
        public T? GetProperty<T>(ApiPropertyPath path) where T : IApiItem
        {
            IApiItem? currentItem = RootItem;
            for (int i = 1; i < path.Length; i++) // Skip implicit root segment
            {
                string pathSegment = path[i];
                if (currentItem is ApiItemDict itemDict)
                {
                    if (itemDict.ContainsKey(pathSegment))
                    {
                        currentItem = itemDict[pathSegment];
                        continue;
                    }
                }
                else if (currentItem is ApiItemList itemList)
                {
                    if (int.TryParse(pathSegment, out int index))
                    {
                        if (index < itemList.Count())
                        {
                            currentItem = itemList[index];
                            continue;
                        }
                    }
                }

                // Arrived here -> not found
                currentItem = null;
            }

            return (currentItem is T tItem) ? tItem : default;
        }

        public T CreateProperty<T>(string fullPath) where T : IApiItem => CreateProperty<T>(ApiPropertyPath.FromFullPath(fullPath), true);
        public T CreateProperty<T>(ApiPropertyPath path) where T : IApiItem => CreateProperty<T>(path, true);
        public T CreateProperty<T>(string fullPath, bool checkPermissions) where T : IApiItem => CreateProperty<T>(ApiPropertyPath.FromFullPath(fullPath), checkPermissions);
        public T CreateProperty<T>(ApiPropertyPath path, bool checkPermissions) where T : IApiItem
        {
            IApiItem? currentItem = RootItem;
            ApiPropertyPath currentPath = ApiPropertyPath.Root;
            for (int i = 1; i < path.Length; i++) // Skip implicit root segment
            {
                bool isLast = i == path.Length - 1;
                string pathSegment = path[i];
                if (currentItem is ApiItemDict itemDict)
                {
                    currentPath = currentPath.Append(pathSegment);
                    if (!PropertyInfos.ContainsKey(currentPath)) throw new ApiInvalidPropertyException(GetType(), currentPath);
                    ApiPropertyInfo currentPropertyInfo = PropertyInfos[currentPath];

                    if (itemDict.ContainsKey(pathSegment))
                    {
                        // Item already exists
                        if (isLast) throw new ApiPropertyAlreadyExistsException(currentPropertyInfo);
                        currentItem = itemDict[pathSegment];
                        continue;
                    }
                    else
                    {
                        // Item doesn't exist yet -> Attempt to create & insert (will throw exception if permissions are missing)
                        IApiItem newItem = ApiItem.CreateNewForProperty(itemDict, currentPropertyInfo);
                        itemDict.Insert(pathSegment, newItem, true);
                        currentItem = newItem;
                        if (isLast) break;
                        continue;
                    }
                }
                else if (currentItem is ApiItemList itemList)
                {
                    currentPath = currentPath.Append("#"); // Append '#' instead of actual index to match against path definitions
                    if (!PropertyInfos.ContainsKey(currentPath)) throw new ApiInvalidPropertyException(GetType(), currentPath);
                    ApiPropertyInfo currentPropertyInfo = PropertyInfos[currentPath];

                    int index = int.Parse(pathSegment);
                    if (index >= 0 && index < itemList.Count())
                    {
                        // Item already exists
                        if (isLast) throw new ApiPropertyAlreadyExistsException(currentPropertyInfo);
                        currentItem = itemList[index];
                        continue;
                    }
                    else
                    {
                        // Item doesn't exist yet -> Attempt to create & insert (will throw exception if permissions are missing)
                        IApiItem newItem = ApiItem.CreateNewForProperty(itemList, currentPropertyInfo);
                        itemList.Insert(newItem);
                        currentItem = newItem;
                        if (isLast) break;
                        continue;
                    }
                }
                else
                {
                    // Something went wrong TODO: Better exception
                    throw new ApiPropertyNotCreatedException(path);
                }
            }

            return (currentItem is T tItem) ? tItem : throw new ApplicationException("Error converting type."); // TODO: Custom Exception
        }

        public T GetOrCreateProperty<T>(string fullPath) where T : IApiItem => GetOrCreateProperty<T>(ApiPropertyPath.FromFullPath(fullPath), true);
        public T GetOrCreateProperty<T>(ApiPropertyPath path) where T : IApiItem => GetOrCreateProperty<T>(path, true);
        public T GetOrCreateProperty<T>(string fullPath, bool checkPermissions) where T : IApiItem => GetOrCreateProperty<T>(ApiPropertyPath.FromFullPath(fullPath), checkPermissions);
        public T GetOrCreateProperty<T>(ApiPropertyPath path, bool checkPermissions) where T : IApiItem
        {
            return GetProperty<T>(path) ?? CreateProperty<T>(path, checkPermissions);
        }

        public void DeleteProperty(string fullPath) => DeleteProperty(ApiPropertyPath.FromFullPath(fullPath));
        public void DeleteProperty(ApiPropertyPath path)
        {
            throw new NotImplementedException();
        }

        public JToken ToJson()
        {
            return RootItem.ToJson();
        }

        public string ToJsonString()
        {
            return RootItem.ToJsonString();
        }

        public ApiResponse ToResponse()
        {
            return RootItem.ToResponse();
        }

        public override string ToString()
        {
            return ResourceName;
        }
    }
}
