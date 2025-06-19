using ApiFramework;

namespace ExampleApi.Resources
{
    internal class ExampleResourceEnumerable : ApiResourceEnumerable<ExampleResource>
    {
        public override IEnumerator<ExampleResource> Enumerator { get; }

        public ExampleResourceEnumerable(IEnumerable<ExampleResource> resources)
        {
            Enumerator = resources.GetEnumerator();
        }
    }
}
