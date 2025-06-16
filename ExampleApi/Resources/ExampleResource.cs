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
        [JsonProperty(PropertyName = "author")]
        private string _author;
        [JsonProperty(PropertyName = "title")]
        private string _title; 
        [JsonProperty(PropertyName = "content")]
        private string _content;
        [JsonProperty(PropertyName = "views")]
        private int _views;
        [JsonProperty(PropertyName = "likes")]
        private int _likes;

        private static string[] _editableItems =
        {
            "author",
            "title",
            "content",
            "views",
            "likes"
        };

        public ExampleResource(JToken json) : base(json)
        {
            _createdAt = DateTime.Now;
        }

        public ExampleResource(string json) : base(json) 
        {
            _createdAt = DateTime.Now;
        }

        public override string GetResourceName()
        {
            return "ExampleResource";
        }

        public override bool CanEditItemCheck(string[] itemPath)
        {
            return _editableItems.Contains(string.Join(".", itemPath));
        }

        public static string ToJson(ExampleResource resource)
        {
            return JsonConvert.SerializeObject(resource);
        }

        public static ExampleResource? FromJson(string json)
        {
            return (ExampleResource?)JsonConvert.DeserializeObject(json, typeof(ExampleResource));
        }
    }
}
