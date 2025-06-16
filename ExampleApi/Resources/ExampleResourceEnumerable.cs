using ApiFramework;

namespace ExampleApi.Resources
{
    internal class ExampleResourceEnumerable : ApiResourceEnumerable<ExampleResource>
    {
        private IEnumerator<ExampleResource> _enumerator;

        protected override IEnumerator<ExampleResource> Enumerator
        {
            get { return _enumerator; }
            set { _enumerator = value; }
        }

        public ExampleResourceEnumerable(IEnumerable<ExampleResource> resources) : base(resources) { }
    }
}
