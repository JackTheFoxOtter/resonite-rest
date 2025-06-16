using ApiFramework;
using ExampleApi;
using Newtonsoft.Json;

ApiServer server = new ApiServer("ExampleApi");

server.RegisterHandler(new ApiEndpoint("GET", "ping"), async (ApiRequest request) =>
{
    return new ApiResponse(200, JsonConvert.SerializeObject("pong"));
});

Task.WaitAll(Task.Run(async () =>
{
    await server.Start("+", 4650);

    ExampleApiTester tester = new ExampleApiTester("localhost", 4650);
    await tester.RunTests();

    while (true)
    {
        await Task.Delay(1000);
    }
}));