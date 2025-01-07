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
        private readonly Uri _baseRoute;
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
            _baseRoute = baseRoute;
            _server = server;
            _endpointQueryResources = new ApiEndpoint("GET",    Utils.JoinUriSegments(baseRoute.ToString(), "/"));
            _endpointSelectResource = new ApiEndpoint("GET",    Utils.JoinUriSegments(baseRoute.ToString(), "/{resourceId}/{...}/"));
            _endpointCreateResource = new ApiEndpoint("POST",   Utils.JoinUriSegments(baseRoute.ToString(), "/{resourceId}/"));
            _endpointUpdateResource = new ApiEndpoint("PATCH",  Utils.JoinUriSegments(baseRoute.ToString(), "/{resourceId}/{...}/"));
            _endpointDeleteResource = new ApiEndpoint("DELETE", Utils.JoinUriSegments(baseRoute.ToString(), "/{resourceId}/"));

            // To query for resources based on query params
            server.RegisterHandler(_endpointQueryResources, async (ApiRequest request) =>
            {
                await CheckRequest(request);

                ApiResourceEnumerable<T> resources = await QueryResources(request.QueryParams);

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

        protected virtual async Task CheckRequest(ApiRequest request) { }

        protected virtual async Task<ApiResourceEnumerable<T>> QueryResources(NameValueCollection queryParams)
        {
            throw new ApiMethodNotSupportedException(_baseRoute.ToString(), "Query");
        }

        protected virtual async Task<T?> SelectResource(string resourceId)
        {
            throw new ApiMethodNotSupportedException(_baseRoute.ToString(), "Select");
        }

        protected virtual async Task<bool> CreateResource(T resource)
        {
            throw new ApiMethodNotSupportedException(_baseRoute.ToString(), "Create");
        }

        protected virtual async Task<bool> UpdateResource(T resource)
        {
            throw new ApiMethodNotSupportedException(_baseRoute.ToString(), "Update");
        }

        protected virtual async Task<bool> DeleteResource(T resource)
        {
            throw new ApiMethodNotSupportedException(_baseRoute.ToString(), "Delete");
        }
    }
}
