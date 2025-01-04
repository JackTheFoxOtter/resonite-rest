using ApiFramework;
using ApiFramework.Resources;
using FrooxEngine;
using SkyFrost.Base;

namespace ResoniteApi.Resources
{
    internal class ContactResourceManager : ApiResourceManager<ContactResource>
    {
        private readonly EngineSkyFrostInterface _cloud;

        public EngineSkyFrostInterface Cloud => _cloud;

        public ContactResourceManager(ApiServer server, string baseUri, EngineSkyFrostInterface cloud) : base(server, baseUri)
        {
            _cloud = cloud;
        }

        public ContactResourceManager(ApiServer server, Uri baseUri, EngineSkyFrostInterface cloud) : base(server, baseUri)
        {
            _cloud = cloud;
        }

        protected override async Task CheckRequest(ApiRequest request)
        {
            Utils.ThrowIfClientIsResonite(request.Context.Request); // Don't allow from within Resonite
        }

        protected override async Task<ApiResourceEnumerable<ContactResource>> GetAllResources()
        {
            IEnumerable<Contact> skyFrostContacts = (await Cloud.Contacts.GetContacts()).Entity;

            return new ContactResourceEnumerable(skyFrostContacts);
        }

        protected override async Task<ContactResource?> GetResource(string resourceId)
        {
            Contact? skyFrostContact = Cloud.Contacts.GetContact(resourceId);
            if (skyFrostContact != null)
            {
                return new ContactResource(skyFrostContact);
            }

            return null;
        }

        protected override async Task CreateResource(ContactResource resource)
        {
            throw new NotImplementedException();
        }

        protected override async Task UpdateResource(ContactResource resource)
        {
            throw new NotImplementedException();
        }
    }
}
