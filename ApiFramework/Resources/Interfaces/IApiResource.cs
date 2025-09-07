using ApiFramework.Resources;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ApiFramework.Resources
{
    public interface IApiResource
    {
        public string ResourceName { get; }
        public IApiProperty RootProperty { get; }
        public void UpdateFrom(ApiResource other);
        public JToken ToJson();
        public string ToJsonString();
        public ApiResponse ToResponse();
    }
}
