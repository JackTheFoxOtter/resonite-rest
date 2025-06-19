using ApiFramework.Interfaces;
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
            await RunTest("Index", TestIndex);
            await RunTest("Ping", TestPing);
            await RunTest("Echo", TestEcho);
            await RunTest("Echo with empty body", TestEchoWithEmptyBody);
            await RunTest("Create a resource", TestCreateResource);
            await RunTest("Create & access a resource", TestCreateAndAccessResource);
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
        /// Tests the index functionality of the API framework.
        /// It should return a JSON list of all registered endpoints with 200 - OK.
        /// (We only test for the "ping" one)
        /// </summary>
        /// <returns>Test successful</returns>
        public async Task<bool> TestIndex()
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
        public async Task<bool> TestPing()
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
        public async Task<bool> TestEcho()
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
        public async Task<bool> TestEchoWithEmptyBody()
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
        public async Task<bool> TestCreateResource()
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
        public async Task<bool> TestCreateAndAccessResource()
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
