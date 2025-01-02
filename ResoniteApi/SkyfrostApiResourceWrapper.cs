using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
namespace ResoniteApi
{
    internal class SkyFrostApiResourceWrapper<T> : ApiResource where T : notnull
    {
        public SkyFrostApiResourceWrapper(T skyFrostResource)
        {
            foreach (var property in skyFrostResource.GetType().GetProperties())
            {
                JsonPropertyAttribute? propertyAttr = property.GetCustomAttribute<JsonPropertyAttribute>();
                if (propertyAttr != null && propertyAttr.PropertyName != null)
                {
                    ObsoleteAttribute? obsoleteAttr = property.GetCustomAttribute<ObsoleteAttribute>();
                    if (obsoleteAttr != null)
                    {
                        continue;
                    }

                    AddItem(propertyAttr.PropertyName, property.GetValue(skyFrostResource));
                }
            }
        }

        public T SkyFrostResource
        {
            get
            {
                T skyFrostResource = Activator.CreateInstance<T>();

                foreach (var property in skyFrostResource.GetType().GetProperties())
                {
                    JsonPropertyAttribute? propertyAttr = property.GetCustomAttribute<JsonPropertyAttribute>();
                    if (propertyAttr != null && propertyAttr.PropertyName != null)
                    {
                        ObsoleteAttribute? obsoleteAttr = property.GetCustomAttribute<ObsoleteAttribute>();
                        if (obsoleteAttr != null)
                        {
                            continue;
                        }

                        property.SetValue(skyFrostResource, this[propertyAttr.PropertyName].Value);
                    }
                }

                return skyFrostResource;
            }
        }
    }
}
