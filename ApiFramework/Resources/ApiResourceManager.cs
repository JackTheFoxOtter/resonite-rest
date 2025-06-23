using ApiFramework.Enums;
using ApiFramework.Exceptions;
using ApiFramework.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private readonly ApiEndpoint _endpointReplaceResource;
        private readonly ApiEndpoint _endpointDeleteResource;

        public string BaseRoute => _baseRoute;
        public ApiServer Server => _server;

        public ApiResourceManager(ApiServer server, string baseRoute, byte methodFlags)
        {
            _baseRoute = baseRoute.Trim('/');
            _server = server;

            if ((methodFlags & (byte)ResourceMethod.Query) > 0)
            {
                // Query for resources based on query params
                _endpointQueryResources = new ApiEndpoint("GET", new Uri(_baseRoute + "/", UriKind.Relative));
                server.RegisterHandler(_endpointQueryResources, async (ApiRequest request) =>
                {
                    await CheckRequest(request);

                    ApiResourceEnumerable<T> resources = await QueryResources(request.QueryParams);

                    return resources.ToResponse();
                });
            }

            if ((methodFlags & (byte)ResourceMethod.Select) > 0)
            {
                // Select & retrieve a known resource (or an item within) directly by id
                _endpointSelectResource = new ApiEndpoint("GET", new Uri(_baseRoute + "/{resourceId}/{...}/", UriKind.Relative));
                server.RegisterHandler(_endpointSelectResource, async (ApiRequest request) =>
                {
                    await CheckRequest(request);

                    string resourceId = request.Arguments[0];
                    string[] itemPath = request.Arguments.Skip(1).ToArray();

                    T resource = await SelectResource(resourceId) ?? throw new ApiResourceNotFoundException(typeof(T), resourceId);
                    if (itemPath.Length == 0)
                    {
                        // Select entire resource
                        return resource.ToResponse();
                    }
                    else
                    {
                        // Select a sub-element of the resource
                        IApiItem item = resource.GetProperty<IApiItem>(new ApiPropertyPath(itemPath)) ?? throw new ApiResourceItemNotFoundException(typeof(T), string.Join(".", itemPath));
                        return item.ToResponse();
                    }
                });
            }

            if ((methodFlags & (byte)ResourceMethod.Create) > 0)
            {
                // Create a new resource
                _endpointCreateResource = new ApiEndpoint("POST", new Uri(_baseRoute + "/", UriKind.Relative));
                server.RegisterHandler(_endpointCreateResource, async (ApiRequest request) =>
                {
                    await CheckRequest(request);

                    string json = await request.GetBody() ?? throw new ApiMissingRequestBodyException();
                    T resource = (T)Activator.CreateInstance(typeof(T), json);
                    
                    string createdResourceId = await CreateResource(resource) ?? throw new ApiResourceNotCreatedException(typeof(T));

                    return new ApiResponse(201, new JObject { ["resourceId"] = createdResourceId }.ToString(Formatting.None)); // 201 - Created
                });
            }

            //if ((methodFlags & (byte)ResourceMethod.Update) > 0)
            //{
            //    // (Partially) update an existing resource
            //    _endpointUpdateResource = new ApiEndpoint("PATCH", new Uri(_baseRoute + "/{resourceId}/{...}/", UriKind.Relative));
            //    server.RegisterHandler(_endpointUpdateResource, async (ApiRequest request) =>
            //    {
            //        await CheckRequest(request);

            //        string json = await request.GetBody() ?? throw new ApiMissingRequestBodyException();
            //        T resource = (T)Activator.CreateInstance(typeof(T), json);


            //    });
            //}

            if ((methodFlags & (byte)ResourceMethod.Replace) > 0)
            {
                // Replace an existing resource fully, or create a new resource
                _endpointCreateResource = new ApiEndpoint("PUT", new Uri(_baseRoute + "/{resourceId}/", UriKind.Relative));
                server.RegisterHandler(_endpointCreateResource, async (ApiRequest request) =>
                {
                    await CheckRequest(request);

                    string resourceId = request.Arguments[0];

                    string json = await request.GetBody() ?? throw new ApiMissingRequestBodyException();
                    T resource = (T)Activator.CreateInstance(typeof(T), json);

                    T? existingResource = await SelectResource(resourceId);
                    if (existingResource != null)
                    {
                        // Update existing
                        if (!await UpdateResource(resource)) throw new ApiResourceNotUpdatedException(typeof(T), resourceId);
                        return new ApiResponse(200, JsonConvert.SerializeObject(new JProperty("resourceId", resourceId))); // 200 - OK
                    }
                    else
                    {
                        // Create new
                        string createdResourceId = await CreateResource(resource) ?? throw new ApiResourceNotCreatedException(typeof(T));
                        return new ApiResponse(201, JsonConvert.SerializeObject(new JProperty("resourceId", createdResourceId))); // 201 - Created
                    }
                });
            }

            if ((methodFlags & (byte)ResourceMethod.Delete) > 0)
            {
                // Delete an existing resource
                _endpointDeleteResource = new ApiEndpoint("DELETE", new Uri(_baseRoute + "/{resourceId}/", UriKind.Relative));
                server.RegisterHandler(_endpointDeleteResource, async (ApiRequest request) =>
                {
                    throw new NotImplementedException();
                });
            }
        }

        protected virtual async Task CheckRequest(ApiRequest request) { }

        protected virtual async Task<ApiResourceEnumerable<T>> QueryResources(NameValueCollection queryParams)
        {
            throw new ApiMethodNotmplementedException(_baseRoute, "Query");
        }

        protected virtual async Task<T?> SelectResource(string resourceId)
        {
            throw new ApiMethodNotmplementedException(_baseRoute, "Select");
        }

        protected virtual async Task<string?> CreateResource(T resource)
        {
            // Should create a new resource from client JSON and on success return the created resource's id.
            throw new ApiMethodNotmplementedException(_baseRoute, "Create");
        }

        protected virtual async Task<bool> UpdateResource(T resource)
        {
            throw new ApiMethodNotmplementedException(_baseRoute, "Update");
        }

        protected virtual async Task<bool> ReplaceResource(T resource)
        {
            throw new ApiMethodNotmplementedException(_baseRoute, "Replace");
        }

        protected virtual async Task<bool> DeleteResource(T resource)
        {
            throw new ApiMethodNotmplementedException(_baseRoute, "Delete");
        }
    }
}
