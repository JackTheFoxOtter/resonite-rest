using ApiFramework.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ResoniteApi
{
    internal abstract class SkyFrostApiResourceWrapper<T> : ApiResource
    {
        public SkyFrostApiResourceWrapper(T skyFrostResource) : base(ToJson(skyFrostResource)) { }

        public SkyFrostApiResourceWrapper(JToken json) : base(json) { }

        public SkyFrostApiResourceWrapper(string json) : base(json) { }

        public T? SkyFrostResource
        {
            get
            {
                return FromJson(ToJson());
            }
        }
        
        public override string GetResourceName()
        {
            return nameof(T);
        }

        public static string ToJson(T skyFrostResource)
        {
            return JsonConvert.SerializeObject(skyFrostResource);
        }

        public static T? FromJson(string json)
        {
            return (T?) JsonConvert.DeserializeObject(json, typeof(T));
        }
    }
}
