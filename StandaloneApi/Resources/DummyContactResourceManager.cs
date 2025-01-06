using ApiFramework;
using ApiFramework.Interfaces;
using ApiFramework.Resources;
using System.Collections.Specialized;

namespace ResoniteApi.Resources
{
    internal class DummyContactResourceManager : ApiResourceManager<DummyContactResource>
    {
        public DummyContactResourceManager(ApiServer server, string baseUri) : base(server, baseUri) { }

        public DummyContactResourceManager(ApiServer server, Uri baseUri) : base(server, baseUri) { }

        protected override async Task CheckRequest(ApiRequest request)
        {
            // No check
        }

        protected override async Task<ApiResourceEnumerable<DummyContactResource>> GetAllResources()
        {
            List<DummyContactResource> contacts = new()
            {
                new DummyContactResource("id0", "Bob", 5),
                new DummyContactResource("id1", "Alice", 5),
                new DummyContactResource("id2", "Kevin", 3)
            };

            return new DummyContactResourceEnumerable(contacts);
        }

        protected override async Task<DummyContactResource?> SelectResource(string resourceId)
        {
            bool predicate(DummyContactResource resource)
            {
                IApiItem? item = resource.GetItemAtPath(new string[] { "ContactId" });
                if (item is ApiItemValue<string> itemValue)
                {
                    return resourceId == itemValue.Value;
                }

                return false;
            }

            DummyContactResource? contactResouce = (from contact in await GetAllResources() where predicate(contact) select contact).FirstOrDefault();

            return contactResouce;
        }

        protected override async Task<bool> CreateResource(DummyContactResource resource)
        {
            throw new NotImplementedException();
        }

        protected override async Task<bool> UpdateResource(DummyContactResource resource)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiResourceEnumerable<DummyContactResource>> QueryResources(NameValueCollection queryParams)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> DeleteResource(DummyContactResource resource)
        {
            throw new NotImplementedException();
        }
    }
}
