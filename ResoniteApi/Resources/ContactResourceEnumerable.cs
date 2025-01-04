using ApiFramework;
using SkyFrost.Base;

namespace ResoniteApi.Resources
{
    internal class ContactResourceEnumerable : ApiResourceEnumerable<ContactResource>
    {
        private IEnumerator<ContactResource> _enumerator;
        
        protected override IEnumerator<ContactResource> Enumerator
        {
            get { return _enumerator; }
            set { _enumerator = value; }
        }

        public ContactResourceEnumerable(IEnumerable<ContactResource> contacts) : base(contacts) { }

        public ContactResourceEnumerable(IEnumerable<Contact> skyFrostContacts) : base(from skyFrostContact in skyFrostContacts select new ContactResource(skyFrostContact)) { }

        //protected static IEnumerable<ContactResource> ResourcesFromSkyFrostContacts(IEnumerable<Contact> skyFrostContacts)
        //{
        //    return from skyFrostContact in skyFrostContacts select new ContactResource(skyFrostContact);
        //}
    }
}
