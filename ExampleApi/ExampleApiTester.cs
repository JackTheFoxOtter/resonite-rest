using Elements.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ExampleApi
{
    internal class ExampleApiTester
    {
        private HttpClient _client;

        public ExampleApiTester(string host, int port)
        {
            _client = new()
            {
                BaseAddress = new Uri($"http://{host}:{port}/ExampleApi/")
            };
        }

        public async Task RunTests()
        {
            await RunTest("Index", TestIndex);
            await RunTest("Ping", TestPing);
        }

        public async Task<bool> RunTest(string testName, Func<Task<bool>> test)
        {
            UniLog.Log($"[Test] Running test: {testName}");
            bool success = await test.Invoke();
            string successText = success ? "PASS" : "FAIL"; 
            UniLog.Log($"[Test] => {successText} of test: {testName}");

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

        public async Task<HttpResponseMessage> HttpRequestWithLogging(HttpRequestMessage message)
        {
            UniLog.Log($"[Test] Request: {message.Method} {message.RequestUri}");
            HttpResponseMessage response = await _client.SendAsync(message);
            UniLog.Log($"[Test] -> Response: {(int) response.StatusCode} - {response.ReasonPhrase}");
            UniLog.Log($"[Test] -> Body: {await response.Content.ReadAsStringAsync()}");

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
