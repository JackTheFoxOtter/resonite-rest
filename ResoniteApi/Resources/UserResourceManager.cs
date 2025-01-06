using ApiFramework;
using ApiFramework.Resources;
using FrooxEngine;
using System.Collections.Specialized;

namespace ResoniteApi.Resources
{
    internal class UserResourceManager : ApiResourceManager<UserResource>
    {
        private readonly EngineSkyFrostInterface _cloud;

        public EngineSkyFrostInterface Cloud => _cloud;

        public UserResourceManager(ApiServer server, string baseUri, EngineSkyFrostInterface cloud) : base(server, baseUri)
        {
            _cloud = cloud;
        }

        public UserResourceManager(ApiServer server, Uri baseUri, EngineSkyFrostInterface cloud) : base(server, baseUri)
        {
            _cloud = cloud;
        }

        protected override async Task CheckRequest(ApiRequest request)
        {
            Utils.ThrowIfClientIsResonite(request.Context.Request); // Don't allow from within Resonite
        }

        protected override async Task<ApiResourceEnumerable<UserResource>> QueryResources(NameValueCollection queryParams)
        {
            string searchName = queryParams.Get("searchName") ?? throw new ArgumentException("Missing required query parameter 'searchName'");
            queryParams.Remove("searchName");

            IEnumerable<SkyFrost.Base.User> skyFrostUsers = (await Cloud.Users.GetUsers(searchName)).Entity;

            return new UserResourceEnumerable(skyFrostUsers).FilterByQueryParams(queryParams);
        }

        protected override async Task<UserResource?> SelectResource(string resourceId)
        {
            SkyFrost.Base.User? skyFrostUser = (await Cloud.Users.GetUser(resourceId)).Entity;
            if (skyFrostUser != null)
            {
                return new UserResource(skyFrostUser);
            }

            return null;
        }

        protected override async Task<bool> CreateResource(UserResource resource)
        {
            throw new NotImplementedException();
        }

        protected override async Task<bool> UpdateResource(UserResource resource)
        {
            throw new NotImplementedException();
        }

        protected override async Task<bool> DeleteResource(UserResource resource)
        {
            throw new NotImplementedException();
        }
    }
}
