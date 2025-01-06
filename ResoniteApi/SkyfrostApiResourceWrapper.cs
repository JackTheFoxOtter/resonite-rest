using ApiFramework.Resources;
using Newtonsoft.Json;

namespace ResoniteApi
{
    internal abstract class SkyFrostApiResourceWrapper<T> : ApiResource
    {
        public SkyFrostApiResourceWrapper(T skyFrostResource) : base(ToJson(skyFrostResource)) { }

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
