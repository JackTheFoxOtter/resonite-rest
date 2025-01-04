using ApiFramework;

namespace ResoniteApi.Resources
{
    internal class DummyContactResourceEnumerable : ApiResourceEnumerable<DummyContactResource>
    {
        private IEnumerator<DummyContactResource> _enumerator;

        protected override IEnumerator<DummyContactResource> Enumerator
        {
            get { return _enumerator; }
            set { _enumerator = value; }
        }

        public DummyContactResourceEnumerable(IEnumerable<DummyContactResource> contacts) : base(contacts) { }
    }
}
