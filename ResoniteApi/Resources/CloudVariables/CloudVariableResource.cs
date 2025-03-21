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

        public override bool CanEditItemCheck(string[] itemPath)
        {
            return _editableItems.Contains(string.Join(".", itemPath));
        }
    }
}
