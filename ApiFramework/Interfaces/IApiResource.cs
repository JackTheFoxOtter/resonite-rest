using ApiFramework.Resources;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ApiFramework.Interfaces
{
    public interface IApiResource
    {
        public string ResourceName { get; }
        public Dictionary<ApiPropertyPath, ApiPropertyInfo> PropertyInfos { get; }
        public IApiItem RootItem { get; }
        public void UpdateFrom(ApiResource other);
        public JToken ToJson();
        public string ToJsonString();
        public ApiResponse ToResponse();
    }
}
