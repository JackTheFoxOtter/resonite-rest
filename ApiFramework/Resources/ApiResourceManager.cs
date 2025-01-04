using System;
using System.Threading.Tasks;
using ApiFramework.Exceptions;
using Elements.Core;

namespace ApiFramework.Resources
{
    public abstract class ApiResourceManager<T> where T : ApiResource
    {
        private readonly ApiServer _server;
        private readonly ApiEndpoint _endpointGetResources;
        private readonly ApiEndpoint _endpointGetResource;
        private readonly ApiEndpoint _endpointGetResourceItem;
        private readonly ApiEndpoint _endpointCreateResource;
        private readonly ApiEndpoint _endpointUpdateResource;
        private readonly ApiEndpoint _endpointUpdateResourceItem;
        private readonly ApiEndpoint _endpointDeleteResource;

        public ApiServer Server => _server;

        public ApiResourceManager(ApiServer server, string baseRoute) : this(server, new Uri(baseRoute, UriKind.Relative)) { }

        public ApiResourceManager(ApiServer server, Uri baseRoute)
        {
            _server = server;

            UniLog.Log($"[ResoniteApi] BaseRoute: '{baseRoute}'");

            _endpointGetResources =       new ApiEndpoint("GET",    Utils.JoinUriSegments(baseRoute.ToString(), "/"));
            _endpointGetResource =        new ApiEndpoint("GET",    Utils.JoinUriSegments(baseRoute.ToString(), "/{resourceId}"));
            _endpointGetResourceItem =    new ApiEndpoint("GET",    Utils.JoinUriSegments(baseRoute.ToString(), "/{resourceId}/{itemName}"));
            _endpointCreateResource =     new ApiEndpoint("PUT",    Utils.JoinUriSegments(baseRoute.ToString(), "/{resourceId}"));
            _endpointUpdateResource =     new ApiEndpoint("PATCH",  Utils.JoinUriSegments(baseRoute.ToString(), "/{resourceId}"));
            _endpointUpdateResourceItem = new ApiEndpoint("PATCH",  Utils.JoinUriSegments(baseRoute.ToString(), "/{resourceId}/{itemName}"));
            _endpointDeleteResource =     new ApiEndpoint("DELETE", Utils.JoinUriSegments(baseRoute.ToString(), "/{resourceId}"));

            server.RegisterHandler(_endpointGetResources, async (ApiRequest request) =>
            {
                await CheckRequest(request);

                ApiResourceEnumerable<T> resources = (await GetAllResources()).FilterByQueryParams(request.QueryParams);

                return resources.ToResponse();
            });

            server.RegisterHandler(_endpointGetResource, async (ApiRequest request) =>
            {
                await CheckRequest(request);

                string requestId = request.Arguments[0];

                T? resource = await GetResource(requestId) ?? throw new ApiResourceNotFoundException(typeof(T), requestId);

                return resource.ToResponse();
            });

            server.RegisterHandler(_endpointGetResourceItem, async (ApiRequest request) =>
            {
                await CheckRequest(request);

                string requestId = request.Arguments[0];
                string itemName = request.Arguments[1];

                T? resource = await GetResource(requestId) ?? throw new ApiResourceNotFoundException(typeof(T), requestId);
                ApiItem? resourceItem = resource[itemName] ?? throw new ApiResourceItemNotFoundException(typeof(T), requestId);
                
                return resourceItem.ToResponse();
            });
        }

        protected abstract Task CheckRequest(ApiRequest request);

        protected abstract Task<ApiResourceEnumerable<T>> GetAllResources();

        protected abstract Task<T?> GetResource(string resourceId);

        protected abstract Task CreateResource(T resource);
        
        protected abstract Task UpdateResource(T resource);
    }
}
