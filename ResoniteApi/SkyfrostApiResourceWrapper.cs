using System.Reflection;
using Newtonsoft.Json;
using ApiFramework.Resources;

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

                    object? propertyValue = property.GetValue(skyFrostResource);
                    if (property.PropertyType.IsEnum && propertyValue != null)
                    {
                        propertyValue = propertyValue.ToString();
                    }

                    AddItem(propertyAttr.PropertyName, propertyValue, property.PropertyType);
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

                        object? propertyValue = this[propertyAttr.PropertyName].Value;
                        if (property.PropertyType.IsEnum && propertyValue != null)
                        {
                            propertyValue = Enum.Parse(property.PropertyType, propertyValue.ToString());
                        }

                        property.SetValue(skyFrostResource, propertyValue);
                    }
                }

                return skyFrostResource;
            }
        }
    }
}
