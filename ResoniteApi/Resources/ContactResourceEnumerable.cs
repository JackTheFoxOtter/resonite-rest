using FrooxEngine;
using SkyFrost.Base;
using System.Collections.Generic;
using System.Linq;

namespace ResoniteApi.Resources
{
    internal class ContactResourceEnumerable : ApiResourceEnumerable<ContactResource>
    {
        private IEnumerator<ContactResource> _enumerator;

        public ContactResourceEnumerable(IEnumerable<Contact> cloudContacts)
        {
            IEnumerable<ContactResource> contacts = from cloudContact in cloudContacts select new ContactResource(cloudContact);
            _enumerator = contacts.GetEnumerator();
        }

        protected override IEnumerator<ContactResource> Enumerator
        {
            get { return _enumerator; }
            set { _enumerator = value; }
        }
    }
}
