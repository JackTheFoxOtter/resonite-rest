using ApiFramework.Interfaces;
using Newtonsoft.Json.Linq;
using System;

namespace ApiFramework.Resources
{
    public abstract class ApiResource : IApiItemContainer
    {
        private readonly IApiItem _rootItem;
        private readonly bool _canEdit;

        public IApiItem RootItem => _rootItem;

        public ApiResource()
        {
            _rootItem = new ApiItemDict(this, CanEditItemCheck(new string[] { }));
            _canEdit = _rootItem.CanEdit();
        }

        public ApiResource(IApiItem rootItem)
        {
            _rootItem = rootItem;
            _canEdit = rootItem.CanEdit();
        }

        public ApiResource(string json)
        {
            _rootItem = ApiItem.FromJson(this, CanEditItemCheck, json);
            _canEdit = _rootItem.CanEdit();
        }

        public ApiResource(JToken json)
        {
            _rootItem = ApiItem.FromJson(this, CanEditItemCheck, json);
            _canEdit = _rootItem.CanEdit();
        }

        public int Count()
        {
            return 1;
        }

        public bool CanEdit()
        {
            return _canEdit;
        }

        public bool Contains(IApiItem item)
        {
            return item == _rootItem;
        }

        public IApiItem? GetItemAtPath(string[] itemPath)
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

        public bool ContainsItemAtPath(string[] itemPath)
        {
            IApiItem? item = GetItemAtPath(itemPath);
            return item != null;
        }

        public string NameOf(IApiItem item)
        {
            if (item != _rootItem) throw new ArgumentException($"ApiResource doesn't contain item {item}");
            return ToString();
        }

        public JToken ToJsonRepresentation()
        {
            return _rootItem.ToJsonRepresentation();
        }

        public string ToJson()
        {
            return _rootItem.ToJson();
        }

        public ApiResponse ToResponse()
        {
            return _rootItem.ToResponse();
        }

        public virtual bool CanEditItemCheck(string[] itemPath)
        {
            return true;
        }

        public override string ToString()
        {
            return GetResourceName();
        }

        public abstract string GetResourceName();
    }
}
