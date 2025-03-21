using ApiFramework;
using SkyFrost.Base;

namespace ResoniteApi.Resources.CloudVariables
{
    internal class CloudVariableResourceEnumerable : ApiResourceEnumerable<CloudVariableResource>
    {
        private IEnumerator<CloudVariableResource> _enumerator;

        protected override IEnumerator<CloudVariableResource> Enumerator
        {
            get { return _enumerator; }
            set { _enumerator = value; }
        }

        public CloudVariableResourceEnumerable(IEnumerable<CloudVariableResource> contacts) : base(contacts) { }

        public CloudVariableResourceEnumerable(IEnumerable<CloudVariable> skyFrostCloudVars) : base(from cloudVar in skyFrostCloudVars select new CloudVariableResource(cloudVar)) { }
    }
}
