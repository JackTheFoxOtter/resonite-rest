using ApiFramework.Exceptions;
using ApiFramework.Interfaces;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace ApiFramework.Resources
{
    public abstract class ApiResourceManager<T> where T : ApiResource
    {
        private readonly ApiServer _server;
        private readonly ApiEndpoint _endpointQueryResources;
        private readonly ApiEndpoint _endpointSelectResource;
        private readonly ApiEndpoint _endpointCreateResource;
        private readonly ApiEndpoint _endpointUpdateResource;
        private readonly ApiEndpoint _endpointDeleteResource;

        public ApiServer Server => _server;

        public ApiResourceManager(ApiServer server, string baseRoute) : this(server, new Uri(baseRoute, UriKind.Relative)) { }

        public ApiResourceManager(ApiServer server, Uri baseRoute)
        {
            _server = server;
            _endpointQueryResources = new ApiEndpoint("GET",    Utils.JoinUriSegments(baseRoute.ToString(), "/"));
            _endpointSelectResource = new ApiEndpoint("GET",    Utils.JoinUriSegments(baseRoute.ToString(), "/{resourceId}/{...}/"));
            _endpointCreateResource = new ApiEndpoint("POST",   Utils.JoinUriSegments(baseRoute.ToString(), "/{resourceId}"));
            _endpointUpdateResource = new ApiEndpoint("PATCH",  Utils.JoinUriSegments(baseRoute.ToString(), "/{resourceId}/{...}/"));
            _endpointDeleteResource = new ApiEndpoint("DELETE", Utils.JoinUriSegments(baseRoute.ToString(), "/{resourceId}"));

            // To query for resources based on query params
            server.RegisterHandler(_endpointQueryResources, async (ApiRequest request) =>
            {
                await CheckRequest(request);

                ApiResourceEnumerable<T> resources = await QueryResources(request.QueryParams); // (await GetAllResources()).FilterByQueryParams(request.QueryParams);

                return resources.ToResponse();
            });

            // To select & retrieve a known resource (or an item within) directly by id
            server.RegisterHandler(_endpointSelectResource, async (ApiRequest request) =>
            {
                await CheckRequest(request);

                string resourceId = request.Arguments[0];
                string[] itemPath = request.Arguments.Skip(1).ToArray();
                
                T? resource = await SelectResource(resourceId) ?? throw new ApiResourceNotFoundException(typeof(T), resourceId);
                if (itemPath.Length == 0)
                {
                    return resource.ToResponse();
                }
                else
                {
                    IApiItem item = resource.GetItemAtPath(itemPath) ?? throw new ApiResourceItemNotFoundException(typeof(T), string.Join(".", itemPath));
                    return item.ToResponse();
                }
            });

            // To create a new resource
            server.RegisterHandler(_endpointCreateResource, async (ApiRequest request) =>
            {
                throw new NotImplementedException();
            });

            // To update an existing resource
            server.RegisterHandler(_endpointUpdateResource, async (ApiRequest request) =>
            {
                throw new NotImplementedException();
            });

            // To delete an existing resource
            server.RegisterHandler(_endpointDeleteResource, async (ApiRequest request) =>
            {
                throw new NotImplementedException();
            });
        }

        protected abstract Task CheckRequest(ApiRequest request);

        protected abstract Task<ApiResourceEnumerable<T>> QueryResources(NameValueCollection queryParams);

        protected abstract Task<T?> SelectResource(string resourceId);

        protected abstract Task<bool> CreateResource(T resource);
        
        protected abstract Task<bool> UpdateResource(T resource);

        protected abstract Task<bool> DeleteResource(T resource);
    }
}
