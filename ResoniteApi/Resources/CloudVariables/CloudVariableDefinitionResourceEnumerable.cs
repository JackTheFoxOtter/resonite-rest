using ApiFramework;
using SkyFrost.Base;

namespace ResoniteApi.Resources.CloudVariables
{
    internal class CloudVariableDefinitionResourceEnumerable : ApiResourceEnumerable<CloudVariableDefinitionResource>
    {
        private IEnumerator<CloudVariableDefinitionResource> _enumerator;

        protected override IEnumerator<CloudVariableDefinitionResource> Enumerator
        {
            get { return _enumerator; }
            set { _enumerator = value; }
        }

        public CloudVariableDefinitionResourceEnumerable(IEnumerable<CloudVariableDefinitionResource> contacts) : base(contacts) { }

        public CloudVariableDefinitionResourceEnumerable(IEnumerable<CloudVariableDefinition> skyFrostCloudVarDefinitions) : base(from cloudVarDefinition in skyFrostCloudVarDefinitions select new CloudVariableDefinitionResource(cloudVarDefinition)) { }
    }
}
