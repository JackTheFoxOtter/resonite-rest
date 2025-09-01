using ApiFramework;
using ApiFramework.Enums;
using ApiFramework.Exceptions;
using ApiFramework.Interfaces;
using ApiFramework.Resources;
using ExampleApi.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace ExampleApi
{
    internal class ExampleApiTester
    {
        public ILogger Logger { get; }
        private HttpClient Client { get; }

        public ExampleApiTester(ILogger logger, string host, int port)
        {
            Logger = logger;
            Client = new()
            {
                BaseAddress = new Uri($"http://{host}:{port}/ExampleApi/")
            };
        }
        
        public async Task RunTests()
        {
            // Data Tests
            await RunTest("Create resource", TestCreateResource);
            //await RunTest("Resource roundtrip simple", TestResourceRoundtripSimple);
            //await RunTest("Resource roundtrip complex", TestResourceRoundtripComplex);

            // API Tests
            //await RunTest("Index", TestIndex);
            //await RunTest("Ping", TestPing);
            //await RunTest("Echo", TestEcho);
            //await RunTest("Echo with empty body", TestEchoWithEmptyBody);
            //await RunTest("Create a resource", TestCreateResource);
            //await RunTest("Create & access a resource", TestCreateAndAccessResource);
        }

        public async Task<bool> RunTest(string testName, Func<Task<bool>> test)
        {
            Logger.Log($"Running test: {testName}");
            bool success = await test.Invoke();
            string successText = success ? "PASS" : "FAIL"; 
            Logger.Log($"=> {successText} of test: {testName}");

            return success;
        }

        /// <summary>
        /// Testing API Item stuff.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> TestItems()
        {

        }

        //public async Task<bool> TestCreateResource()
        //{
        //    ExampleResource resource = new ExampleResource();
        //    Logger.Log($"Created resource {resource}:");
        //    foreach (KeyValuePair<ApiPropertyPath, ApiPropertyInfo> info in resource.PropertyInfos)
        //    {
        //        ApiPropertyPath path = info.Key;
        //        ApiPropertyInfo property = info.Value;

        //        Logger.Log($" -> {path}: Type: {property.TargetType.GetNiceTypeName()}, Perms: {property.Permissions.ToFriendlyName()}");
        //    }
        //    Logger.Log($"Resource JSON: {resource.ToJsonString()}");

        //    // Test modifying a modifiable property
        //    Logger.Log($"Test writing description... (should succeed, description has create & modify permission)");
        //    string new_description = "Test Description";
        //    resource.Description.Value = new_description; // Write to object
        //    string? description = resource?.ToJson()?["description"]?.Value<string>();
        //    if (description != new_description)
        //    {
        //        Logger.Log($"Failed! Description was '{description}', expected: '{new_description}'");
        //        return false;
        //    }

        //    // Test modifying a read-only property (expect failure)
        //    Logger.Log($"Test writing id... (should fail, id is read-only)");
        //    try
        //    {
        //        resource.ID.Value = Guid.NewGuid().ToString();
        //        Logger.Log("Failed! Should have thrown exception on write.");
        //        return false;

        //    } catch(ApiResourceMissingPermissionsException ex) { }

        //    // TODO:
        //    // Test inserting a property that doesn't have create permission (expect failure)
        //    Logger.Log($"Test creating an object... (should fail, object only has modify permission, but cannot be created)");
        //    try
        //    {
        //        ApiItemDict obj = resource.Object;
        //        ApiItemValue<string> objectText = obj.Get<ApiItemValue<string>>("text");
        //        ApiItemValue<int> objInt = obj.Get<ApiItemValue<int>>("number");


        //    } catch(ApiResourceMissingPermissionsException ex) { }


        //    return true;
        //}

        /// <summary>
        /// Tests a full round-trip of an ExampleResource loaded from a JSON string.
        /// Simple version - lists & objects are empty.
        /// </summary>
        /// <returns>Test successful</returns>
        public async Task<bool> TestResourceRoundtripSimple()
        {
            string inputJsonStr = "{ id: 'ID00001', createDate: '2012-04-23T18:25:43.511Z', updateDate: '2012-04-23T18:25:43.511Z', name: 'Test Resource', 'description': null, 'listCount': 0, 'list': [], 'object': {} }";
            JToken? inputJson = JsonConvert.DeserializeObject<JToken>(inputJsonStr);
            Logger.Log($"Input: {inputJson?.ToString()}");
            ExampleResource resource = new(inputJsonStr);
            JToken outputJson = resource.ToJson();
            Logger.Log($"Output: {outputJson.ToString()}");

            return JToken.DeepEquals(inputJson, outputJson);
        }

        public async Task<bool> TestResourceRoundtripComplex()
        {
            string inputJsonStr = "{id: 'ID00002', name: 'Test Resource 2', 'listCount': 3, 'list': [12, 'Hello!', true], 'object': { 'a': 12, 'b': 'Hello!', 'c': true }}";
            JToken? inputJson = JsonConvert.DeserializeObject<JToken>(inputJsonStr);
            Logger.Log($"Input: {inputJson?.ToString()}");
            ExampleResource resource = new(inputJsonStr);
            JToken outputJson = resource.ToJson();
            Logger.Log($"Output: {outputJson.ToString()}");

            return JToken.DeepEquals(inputJson, outputJson);
        }

        // TODO:
        // - Lists
        // - Dicts
        // - Update item that exists
        // - Update item that doesn't exist
        // - Try update item that is read-only
        // - Delete item that exists
        // - Try delete item that is read-only
        // - Partially update resource from another resource
        // - Fully update resource from another resource

        /// <summary>
        /// Tests the index functionality of the API framework.
        /// It should return a JSON list of all registered endpoints with 200 - OK.
        /// (We only test for the "ping" one)
        /// </summary>
        /// <returns>Test successful</returns>
        public async Task<bool> TestApiIndex()
        {
            HttpResponseMessage response = await HttpRequestWithLogging(new HttpRequestMessage(HttpMethod.Get, ""));
            JToken? responseJson = await ReadResponseAsJson(response);

            bool success = response.IsSuccessStatusCode
                && responseJson is JArray array
                && array.Count > 0
                && array.Where(token => JToken.DeepEquals(token, "GET ping")).Any();

            return success;
        }

        /// <summary>
        /// Tests the "ping" route of the Example API.
        /// Should return "pong" with 200 - OK.
        /// </summary>
        /// <returns>Test successful</returns>
        public async Task<bool> TestApiPing()
        {
            HttpResponseMessage response = await HttpRequestWithLogging(new HttpRequestMessage(HttpMethod.Get, "ping"));
            JToken? responseJson = await ReadResponseAsJson(response);

            bool success = response.IsSuccessStatusCode
                && JToken.DeepEquals(responseJson, "pong");

            return success;
        }

        /// <summary>
        /// Tests the "echo" route of the Example API.
        /// Should return a 200 - OK response with the same payload as sent in the request.
        /// </summary>
        /// <returns>Test successful</returns>
        public async Task<bool> TestApiEcho()
        {
            string payload = "I'm a payload!";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "echo");
            request.Content = new System.Net.Http.StringContent(payload, System.Text.Encoding.UTF8, "application/text");
            HttpResponseMessage response = await HttpRequestWithLogging(request);
            string? responseString = await response.Content.ReadAsStringAsync();

            bool success = response.IsSuccessStatusCode
                && responseString == payload;

            return success;
        }

        /// <summary>
        /// Tests the "echo" route of the Example API with an empty body.
        /// Should fail with 400 - Bad Request and a message indicating that the body cannot be empty.
        /// </summary>
        /// <returns>Test successful</returns>
        public async Task<bool> TestApiEchoWithEmptyBody()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "echo");
            request.Content = new System.Net.Http.StringContent(String.Empty, System.Text.Encoding.UTF8, "application/text");
            HttpResponseMessage response = await HttpRequestWithLogging(request);
            JToken? responseJson = await ReadResponseAsJson(response);

            bool success = response.StatusCode == System.Net.HttpStatusCode.BadRequest
                && JToken.DeepEquals(responseJson, "Request body cannot be empty.");

            return success;
        }

        /// <summary>
        /// Tests inserting a new Resource.
        /// Should succeed with 201 - Created and contain a JSON object with the new resourceId in the response body.
        /// </summary>
        /// <returns>Test successful</returns>
        public async Task<bool> TestApiCreateResource()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "exampleResource/");
            request.Content = new System.Net.Http.StringContent("{ name: 'TestCreateResource', counter: 0 }");
            HttpResponseMessage response = await HttpRequestWithLogging(request);
            JToken? responseJson = await ReadResponseAsJson(response);

            bool success = response.IsSuccessStatusCode
                && responseJson is JObject obj
                && obj.ContainsKey("resourceId");

            return success;
        }

        /// <summary>
        /// Tests inserting a new resource & immediately accessing it again.
        /// Should success with 200 - OK and contian the resource as a JSON object in the response body.
        /// </summary>
        /// <returns>Test successful</returns>
        public async Task<bool> TestApiCreateAndAccessResource()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "exampleResource/");
            request.Content = new System.Net.Http.StringContent("{ name: 'TestCreateAndAccessResource', counter: 0 }");
            HttpResponseMessage response = await HttpRequestWithLogging(request);
            JToken? responseJson = await ReadResponseAsJson(response);

            if (responseJson == null || responseJson is not JObject obj || !obj.ContainsKey("resourceId") ) return false;

            string resourceId = obj["resourceId"].Value<string>();
            HttpRequestMessage request2 = new HttpRequestMessage(HttpMethod.Get, $"exampleResource/{resourceId}");
            HttpResponseMessage response2 = await HttpRequestWithLogging(request2);
            JToken? responseJson2 = await ReadResponseAsJson(response2);

            bool success = response2.IsSuccessStatusCode;

            return success;
        }

        public async Task<HttpResponseMessage> HttpRequestWithLogging(HttpRequestMessage message)
        {
            Logger.Log($"-> Request: {message.Method} {message.RequestUri}");
            HttpResponseMessage response = await Client.SendAsync(message);
            Logger.Log($"-> Response: {(int) response.StatusCode} - {response.StatusCode}");
            Logger.Log($"-> Body: {await response.Content.ReadAsStringAsync()}");

            return response;
        }

        public async Task<JToken?> ReadResponseAsJson(HttpResponseMessage response)
        {
            string content = await response.Content.ReadAsStringAsync();
            JToken? json = JsonConvert.DeserializeObject<JToken>(content);
            return json;
        }

    }
}
