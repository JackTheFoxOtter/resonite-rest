using ApiFramework;
using ApiFramework.Enums;
using ApiFramework.Exceptions;
using ApiFramework.Resources;
using System.Collections.Specialized;

namespace ExampleApi.Resources
{
    internal class ExampleResourceManager : ApiResourceManager<ExampleResource>
    {
        // Resources are only stored in-memory for this example manager.
        // A proper implementation might store them in a database or host application.
        // This dictionary maps resource IDs to JSON strings of resources.
        private Dictionary<string, string> _resources = new Dictionary<string, string>();

        public ExampleResourceManager(ApiServer server, string baseRoute) : base(server, baseRoute, (byte)ResourceMethod.All) { }

        protected override async Task<ApiResourceEnumerable<ExampleResource>> QueryResources(NameValueCollection queryParams)
        {
            throw new ApiMethodNotmplementedException(BaseRoute, "Query");
        }

        protected override async Task<ExampleResource?> SelectResource(string resourceId)
        {
            string json = _resources[resourceId] ?? throw new ApiResourceNotFoundException(typeof(ExampleResource), resourceId);
            ExampleResource resource = new ExampleResource(json);

            return resource;
            throw new ApiMethodNotmplementedException(BaseRoute, "Select");
        }

        protected override async Task<string?> CreateResource(ExampleResource resource)
        {
            string new_id = Guid.NewGuid().ToString();

            resource.ID = new_id;
            _resources.Add(new_id, resource.ToJsonString());

            return new_id;
        }

        protected override async Task<bool> UpdateResource(ExampleResource resource)
        {
            throw new ApiMethodNotmplementedException(BaseRoute, "Update");
        }

        protected override async Task<bool> ReplaceResource(ExampleResource resource)
        {
            throw new ApiMethodNotmplementedException(BaseRoute, "Replace");
        }

        protected override async Task<bool> DeleteResource(ExampleResource resource)
        {
            throw new ApiMethodNotmplementedException(BaseRoute, "Delete");
        }
    }
}
