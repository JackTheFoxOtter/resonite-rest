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
        private readonly string _baseRoute;
        private readonly ApiServer _server;
        private readonly ApiEndpoint _endpointQueryResources;
        private readonly ApiEndpoint _endpointSelectResource;
        private readonly ApiEndpoint _endpointCreateResource;
        private readonly ApiEndpoint _endpointUpdateResource;
        private readonly ApiEndpoint _endpointDeleteResource;

        public ApiServer Server => _server;

        public ApiResourceManager(ApiServer server, string baseRoute)
        {
            _baseRoute = baseRoute.Trim('/');
            _server = server;
            _endpointQueryResources = new ApiEndpoint("GET",    new Uri(_baseRoute + "/",                    UriKind.Relative));
            _endpointSelectResource = new ApiEndpoint("GET",    new Uri(_baseRoute + "/{resourceId}/{...}/", UriKind.Relative));
            _endpointCreateResource = new ApiEndpoint("POST",   new Uri(_baseRoute + "/{resourceId}/",       UriKind.Relative));
            _endpointUpdateResource = new ApiEndpoint("PATCH",  new Uri(_baseRoute + "/{resourceId}/{...}/", UriKind.Relative));
            _endpointDeleteResource = new ApiEndpoint("DELETE", new Uri(_baseRoute + "/{resourceId}/",       UriKind.Relative));

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
            throw new ApiMethodNotSupportedException(_baseRoute, "Query");
        }

        protected virtual async Task<T?> SelectResource(string resourceId)
        {
            throw new ApiMethodNotSupportedException(_baseRoute, "Select");
        }

        protected virtual async Task<bool> CreateResource(T resource)
        {
            throw new ApiMethodNotSupportedException(_baseRoute, "Create");
        }

        protected virtual async Task<bool> UpdateResource(T resource)
        {
            throw new ApiMethodNotSupportedException(_baseRoute, "Update");
        }

        protected virtual async Task<bool> DeleteResource(T resource)
        {
            throw new ApiMethodNotSupportedException(_baseRoute, "Delete");
        }
    }
}
