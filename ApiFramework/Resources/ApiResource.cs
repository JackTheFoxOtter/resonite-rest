using ApiFramework.Enums;
using ApiFramework.Interfaces;
using Newtonsoft.Json.Linq;
using System;

namespace ApiFramework.Resources
{
    public abstract class ApiResource : IApiResource, IApiItemContainer
    {
        public IApiItem RootItem { get; }

        public ApiResource(IApiItem rootItem)
        {
            RootItem = rootItem;
        }

        public ApiResource(JToken json)
        {
            RootItem = ApiItem.FromJson(this, GetItemPermissions, json);
        }

        public ApiResource(string json)
        {
            RootItem = ApiItem.FromJson(this, GetItemPermissions, json);
        }

        public abstract string GetResourceName();

        public abstract EditPermission GetItemPermissions(string[] path);

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
            return ToString();
        }

        public IApiItem? GetItemAtPath(params string[] itemPath)
        {
            IApiItem? item = RootItem;
            foreach (string pathSegment in itemPath)
            {
                if (item is ApiItemDict itemDict)
                {
                    if (itemDict.ContainsKey(pathSegment))
                    {
                        item = itemDict[pathSegment];
                        continue;
                    }
                }
                else if (item is ApiItemList itemList)
                {
                    if (int.TryParse(pathSegment, out int index))
                    {
                        if (index < itemList.Count())
                        {
                            item = itemList[index];
                            continue;
                        }
                    }
                }

                // Arrived here -> not found
                item = null;
            }

            return item;
        }

        public T? GetItemAtPath<T>(params string[] itemPath) where T: IApiItem
        {
            IApiItem? item = GetItemAtPath(itemPath);
            return (item is T tItem) ? tItem : default;
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
            return GetResourceName();
        }
    }
}
