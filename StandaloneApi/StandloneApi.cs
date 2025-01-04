using ApiFramework;
using Newtonsoft.Json;
using ResoniteApi.Resources;

ApiServer server = new ApiServer();

server.RegisterHandler(new ApiEndpoint("GET", "ping"), async (ApiRequest request) =>
{
    string response = "pong";

    return new ApiResponse(200, JsonConvert.SerializeObject(response));
});

server.RegisterHandler(new ApiEndpoint("GET", "version"), async (ApiRequest request) =>
{
    string version = "Standalone Test API Client";

    return new ApiResponse(200, JsonConvert.SerializeObject(version));
});

DummyContactResourceManager dummyContactManager = new(server, "contacts");

Task.WaitAll(Task.Run(async () =>
{ 
    server.Start(4600);

    while(true)
    {
        await Task.Delay(1);
    }
}));

