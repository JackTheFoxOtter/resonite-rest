using ApiFramework;
using ApiFramework.Exceptions;
using FrooxEngine;
using Newtonsoft.Json;
using ResoniteApi.Exceptions;
using SkyFrost.Base;

namespace ResoniteApi.Resources.CloudVariables
{
    public class CloudVariableResourceManager
    {
        private readonly string _baseRoute;
        private readonly ApiServer _server;
        private readonly EngineSkyFrostInterface _cloud;
        private readonly ApiEndpoint _endpointQueryCloudVars;
        private readonly ApiEndpoint _endpointListDefinitions;
        private readonly ApiEndpoint _endpointGetDefinition;
        private readonly ApiEndpoint _endpointCreateDefinition;
        private readonly ApiEndpoint _endpointDeleteDefinition;
        private readonly ApiEndpoint _endpointGetVariable;
        private readonly ApiEndpoint _endpointSetVariable;

        public ApiServer Server => _server;
        public EngineSkyFrostInterface Cloud => _cloud;

        public CloudVariableResourceManager(ApiServer server, string baseRoute, EngineSkyFrostInterface cloud)
        {
            _baseRoute = baseRoute.Trim('/');
            _server = server;
            _cloud = cloud;
            _endpointQueryCloudVars = new ApiEndpoint("GET", new Uri(_baseRoute + "/", UriKind.Relative));
            _endpointListDefinitions = new ApiEndpoint("GET", new Uri(_baseRoute + "/{definitionOwnerId}/", UriKind.Relative));
            _endpointGetDefinition = new ApiEndpoint("GET", new Uri(_baseRoute + "/{definitionOwnerId}/{subPath}/", UriKind.Relative));
            //_endpointCreateDefinition = new ApiEndpoint("POST",   new Uri(_baseRoute + "/{definitionOwnerId}/{subPath}/",                  UriKind.Relative));
            //_endpointDeleteDefinition = new ApiEndpoint("DELETE", new Uri(_baseRoute + "/{definitionOwnerId}/{subPath}/",                  UriKind.Relative));
            _endpointGetVariable = new ApiEndpoint("GET", new Uri(_baseRoute + "/{definitionOwnerId}/{subPath}/{variableOwnerId}", UriKind.Relative));
            _endpointSetVariable = new ApiEndpoint("POST", new Uri(_baseRoute + "/{definitionOwnerId}/{subPath}/{variableOwnerId}", UriKind.Relative));

            // To query for cloud vars by owner
            server.RegisterHandler(_endpointQueryCloudVars, async (ApiRequest request) =>
            {
                await CheckRequest(request);

                string ownerId = request.QueryParams.PopJsonParam<string>("ownerId") ?? _cloud.CurrentUserID;

                List<CloudVariable> skyFrostVariables = (await _cloud.Variables.GetAllByOwner(ownerId)).Entity ?? new();

                CloudVariableResourceEnumerable variables = new(skyFrostVariables);
                CloudVariableResourceEnumerable filtered = (CloudVariableResourceEnumerable) variables.FilterByQueryParams(request.QueryParams);

                return filtered.ToResponse();
            });

            // To list all definitions for a specified owner
            server.RegisterHandler(_endpointListDefinitions, async (ApiRequest request) =>
            {
                await CheckRequest(request);

                string definitionOwnerId = request.Arguments[0];

                List<CloudVariableDefinition> skyFrostDefinitions = (await _cloud.Variables.ListDefinitions(definitionOwnerId)).Entity ?? new();
                CloudVariableDefinitionResourceEnumerable definitions = new(skyFrostDefinitions);

                return definitions.ToResponse();
            });

            // To retrieve a specific definition
            server.RegisterHandler(_endpointGetDefinition, async (ApiRequest request) =>
            {
                await CheckRequest(request);

                string definitionOwnerId = request.Arguments[0];
                string subPath = request.Arguments[1];

                CloudVariableDefinition skyFrostDefinition = (await _cloud.Variables.GetDefinition(definitionOwnerId, subPath)).Entity 
                    ?? throw new ApiResourceNotFoundException(typeof(CloudVariableDefinitionResource), $"{definitionOwnerId}.{subPath}");

                CloudVariableDefinitionResource definition = new(skyFrostDefinition);

                return definition.ToResponse();
            });

            // To retrieve a specific variable for a specific user
            server.RegisterHandler(_endpointGetVariable, async (ApiRequest request) =>
            {
                await CheckRequest(request);

                string definitionOwnerId = request.Arguments[0];
                string subPath = request.Arguments[1];
                string variableOwnerId = request.Arguments[2];

                string path = $"{definitionOwnerId}.{subPath}";
                if (!CloudVariableHelper.IsValidPath(path)) throw new ApiInvalidCloudVariablePathException(path);



                CloudVariableProxy proxy = _cloud.Variables.RequestProxy(variableOwnerId, path);
                await proxy.Refresh();
                if (proxy.State is CloudVariableState.Invalid or CloudVariableState.Unregistered)
                    throw new ApiInvalidCloudVariableProxyException();

                var value = proxy.ReadValue<object>();
                CloudVariableResource variable = CloudVariableResource.FromPartial(variableOwnerId, path, value, null);

                return variable.ToResponse();
            });

            // To update a specific variable for a specific user
            server.RegisterHandler(_endpointSetVariable, async (ApiRequest request) =>
            {
                await CheckRequest(request);

                string definitionOwnerId = request.Arguments[0];
                string subPath = request.Arguments[1];
                string variableOwnerId = request.Arguments[2];

                string path = $"{definitionOwnerId}.{subPath}";
                if (!CloudVariableHelper.IsValidPath(path)) throw new ApiInvalidCloudVariablePathException(path);

                string body = await request.GetBody() ?? throw new ApiMissingRequestBodyException();
                object? parsedValue = JsonConvert.DeserializeObject(body);
                if (parsedValue == null || parsedValue.GetType() != typeof(string)) 
                    throw new ApiJsonParsingException($"Failed to parse request body. (Expected JSON-formatted {typeof(string)} value)");

                CloudVariableProxy proxy = _cloud.Variables.RequestProxy(variableOwnerId, path);
                await proxy.Refresh();
                if (proxy.State is CloudVariableState.Invalid or CloudVariableState.Unregistered)
                    throw new ApiInvalidCloudVariableProxyException();

                if (!CloudVariableHelper.IsValidValue(proxy._definition.VariableType, (string) parsedValue))
                    throw new ApiInvalidCloudVariableValueException(proxy._definition.VariableType, (string) parsedValue);

                if (!proxy.SetValue((string) parsedValue))
                    throw new ApiCloudVariableWriteException();

                return new ApiResponse(200, JsonConvert.SerializeObject("Cloud variable value updated."));
            });
        }

        private async Task CheckRequest(ApiRequest request)
        {
            Utils.ThrowIfClientIsResonite(request.Context.Request); // Don't allow from within Resonite
        }
    }
}