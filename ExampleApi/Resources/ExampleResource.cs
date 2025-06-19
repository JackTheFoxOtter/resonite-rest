using ApiFramework.Enums;
using ApiFramework.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ExampleApi.Resources
{
    internal class ExampleResource : ApiResource
    {
        [JsonProperty(PropertyName = "id")]
        private string _id;
        [JsonProperty(PropertyName = "createdAt")]
        private DateTime _createdAt;
        [JsonProperty(PropertyName = "updatedAt")]
        private DateTime _updatedAt;
        [JsonProperty(PropertyName = "name")]
        private string _name;
        [JsonProperty(PropertyName = "counter")]
        private string _counter;

        public string ID { get => _id; set => _id = value; }
        public DateTime CreatedAt { get => _createdAt; set => _createdAt = value; }
        public DateTime UpdatedAt { get => _updatedAt; set => _updatedAt = value; }
        public string Name { get => _name; set => _name = value; }
        public string Counter { get => _counter; set => _counter = value; }

        public ExampleResource(JToken json) : base(json)
        {
            _createdAt = DateTime.Now;
            _updatedAt = DateTime.Now;
        }

        public ExampleResource(string json) : base(json) 
        {
            _createdAt = DateTime.Now;
            _updatedAt = DateTime.Now;
        }
        public override string GetResourceName()
        {
            return "ExampleResource";
        }

        private readonly static string[] _createableItems = { "id", "name", "counter" };
        private readonly static string[] _modifiableItems = { "name", "counter" };
        private readonly static string[] _deleteableItems = { "name", "counter" };

        public override EditPermission GetItemPermissions(string[] itemPath)
        {
            string fullPath = string.Join(".", itemPath);
            EditPermission perms = EditPermission.None;

            if (_createableItems.Contains(fullPath)) perms |= EditPermission.Create;
            if (_modifiableItems.Contains(fullPath)) perms |= EditPermission.Modify;
            if (_deleteableItems.Contains(fullPath)) perms |= EditPermission.Delete;

            return perms;
        }

        public static string ToJson(ExampleResource resource)
        {
            return JsonConvert.SerializeObject(resource);
        }

        public static ExampleResource? FromJson(string json)
        {
            return JsonConvert.DeserializeObject<ExampleResource>(json);
        }
    }
}
