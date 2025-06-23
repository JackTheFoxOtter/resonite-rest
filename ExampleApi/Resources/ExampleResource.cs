using ApiFramework.Enums;
using ApiFramework.Interfaces;
using ApiFramework.Resources;
using Newtonsoft.Json.Linq;

namespace ExampleApi.Resources
{
    internal class ExampleResource : ApiResource
    {
        public ApiItemValue<string> ID => GetOrCreateProperty<ApiItemValue<string>>("id", true);
        public ApiItemValue<DateTime> CreateDate => GetOrCreateProperty<ApiItemValue<DateTime>>("createDate", true);
        public ApiItemValue<DateTime> UpdateDate => GetOrCreateProperty<ApiItemValue<DateTime>>("updateDate", true);
        public ApiItemValue<string> Name => GetOrCreateProperty<ApiItemValue<string>>("name", true);
        public ApiItemValue<string> Description => GetOrCreateProperty<ApiItemValue<string>>("description", true);
        public ApiItemValue<int> ListCount => GetOrCreateProperty<ApiItemValue<int>>("listCount", true);
        public ApiItemList List => GetOrCreateProperty<ApiItemList>("list", true);
        public ApiItemDict Dict => GetOrCreateProperty<ApiItemDict>("object", true);

        public ExampleResource() : base() { }
        public ExampleResource(string json) : base(json) { }
        public ExampleResource(JToken json) : base(json) { }
        public ExampleResource(IApiItem? rootItem, JToken? json) : base(rootItem, json) { }

        protected override ApiPropertyInfo[] GetPropertyInfos()
        {
            return new ApiPropertyInfo[]
            {
                new ApiPropertyInfo(this, ".id", typeof(ApiItemValue<string>), EditPermission.None),
                new ApiPropertyInfo(this, ".createDate", typeof(ApiItemValue<DateTime>), EditPermission.None),
                new ApiPropertyInfo(this, ".updateDate", typeof(ApiItemValue<DateTime>), EditPermission.None),
                new ApiPropertyInfo(this, ".name", typeof(ApiItemValue<string>), EditPermission.CreateModify),
                new ApiPropertyInfo(this, ".description", typeof(ApiItemValue<string>), EditPermission.CreateModifyDelete),
                new ApiPropertyInfo(this, ".listCount", typeof(ApiItemValue<int>), EditPermission.Modify),
                new ApiPropertyInfo(this, ".list", typeof(ApiItemList), EditPermission.Modify),
                new ApiPropertyInfo(this, ".list.#", typeof(ApiItemValue<object>), EditPermission.CreateModifyDelete),
                new ApiPropertyInfo(this, ".object", typeof(ApiItemDict), EditPermission.Modify),
                new ApiPropertyInfo(this, ".object.text", typeof(ApiItemValue<string>), EditPermission.Modify),
                new ApiPropertyInfo(this, ".object.number", typeof(ApiItemValue<int>), EditPermission.Modify),
            };
        }
    }
}
