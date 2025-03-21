using SkyFrost.Base;

namespace ResoniteApi.Resources.CloudVariables
{
    internal class CloudVariableDefinitionResource : SkyFrostApiResourceWrapper<CloudVariableDefinition>
    {
        private static string[] _editableItems =
        {
            "variableType",
            "defaultValue",
            "readPermissions",
            "writePermissions",
            "listPermissions"
        };

        public CloudVariableDefinitionResource(CloudVariableDefinition skyFrostCloudVariableDefinition) : base(skyFrostCloudVariableDefinition) { }

        public override bool CanEditItemCheck(string[] itemPath)
        {
            return _editableItems.Contains(string.Join(".", itemPath));
        }
    }
}
