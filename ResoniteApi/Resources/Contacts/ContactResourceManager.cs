using ApiFramework;
using ApiFramework.Enums;
using ApiFramework.Exceptions;
using ApiFramework.Resources;
using FrooxEngine;
using SkyFrost.Base;
using System.Collections.Specialized;

namespace ResoniteApi.Resources.Contacts
{
    internal class ContactResourceManager : ApiResourceManager<ContactResource>
    {
        private readonly EngineSkyFrostInterface _cloud;

        public EngineSkyFrostInterface Cloud => _cloud;

        public ContactResourceManager(ApiServer server, string baseUri, EngineSkyFrostInterface cloud) : base(server, baseUri, (byte)ResourceMethod.All)
        {
            _cloud = cloud;
        }

        protected override async Task CheckRequest(ApiRequest request)
        {
            Utils.ThrowIfClientIsResonite(request.Context.Request); // Don't allow from within Resonite
        }

        protected override async Task<ApiResourceEnumerable<ContactResource>> QueryResources(NameValueCollection queryParams)
        {
            IEnumerable<Contact> skyFrostContacts = (await Cloud.Contacts.GetContacts()).Entity;

            return new ContactResourceEnumerable(skyFrostContacts).FilterByQueryParams(queryParams);
        }

        protected override async Task<ContactResource?> SelectResource(string resourceId)
        {
            Contact? skyFrostContact = Cloud.Contacts.GetContact(resourceId);
            if (skyFrostContact != null)
            {
                return new ContactResource(skyFrostContact);
            }

            return null;
        }

        protected override async Task<string?> CreateResource(ContactResource contact)
        {
            string contactUserId = contact.GetItemAtPath<ApiItemValue<string>>("id")?.Value ?? throw new ApiMissingRequiredArgumentException("id");
            
            if (await Cloud.Contacts.AddContact(contactUserId, string.Empty))
            {
                return contactUserId;
            }

            return null;
        }

        protected override async Task<bool> UpdateResource(ContactResource contact)
        {
            throw new NotImplementedException();
            //Contact skyFrostContact = resource.SkyFrostResource;
            //return await Cloud.Contacts.RemoveContact(skyFrostContact);
        }

        protected override async Task<bool> DeleteResource(ContactResource resource)
        {
            throw new NotImplementedException();
        }
    }
}
