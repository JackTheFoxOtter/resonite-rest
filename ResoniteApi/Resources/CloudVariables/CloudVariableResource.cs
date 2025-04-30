using Newtonsoft.Json.Linq;
using SkyFrost.Base;

namespace ResoniteApi.Resources.CloudVariables
{
    internal class CloudVariableResource : SkyFrostApiResourceWrapper<CloudVariable>
    {
        private static string[] _editableItems =
        {
            "value"
        };

        public CloudVariableResource(CloudVariable skyFrostCloudVariable) : base(skyFrostCloudVariable) { }

        public CloudVariableResource(JToken json) : base(json) { }

        public CloudVariableResource(string json) : base(json) { }

        public override bool CanEditItemCheck(string[] itemPath)
        {
            return _editableItems.Contains(string.Join(".", itemPath));
        }

        public static CloudVariableResource FromPartial(string? ownerId, string? path, object? value, DateTime? timestamp)
        {
            JToken json = new JObject(
                new JProperty("ownerId", ownerId),
                new JProperty("path", path),
                new JProperty("value", value),
                new JProperty("timestamp", timestamp)
            );

            return new CloudVariableResource(json);
        }
    }
}
