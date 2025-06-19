using ApiFramework;
using ApiFramework.Exceptions;
using ExampleApi;
using ExampleApi.Resources;
using Newtonsoft.Json;

// Simple logger that just outputs to the console.
ConsoleLogger serverLogger = new ConsoleLogger("ApiServer");

// Creates a new APIServer instance with the base route "/ExampleApi".
ApiServer server = new ApiServer(serverLogger, "ExampleApi");

// Registers a simple handler for a GET request to the "/ping" endpoint.
server.RegisterHandler(new ApiEndpoint("GET", "ping"), async (ApiRequest request) =>
{
    // When called, the endpoint will return a response with status code 200 and a body containing "pong" as a JSON string.
    return new ApiResponse(200, JsonConvert.SerializeObject("pong"));
});

// Registers a handler for a POST request to the "/echo" endpoint.
server.RegisterHandler(new ApiEndpoint("POST", "echo"), async (ApiRequest request) =>
{
    // Attempt to load the body of the incoming request as a string.
    string body = await request.GetBody()
        // If the body is null, throw an exception.
        // Any exception extending ApiException will be caught by the server and result in an appropriate HTTP error.
        ?? throw new ApiMissingRequestBodyException();

    // Return a response with status code 200 and a body echoing the content of the incoming request.
    return new ApiResponse(200, body);
});

// Creates a new ResourceManager for the resource ExampleResource.
// Resources represent data entities that can be accessed via common RESTful API endpoints.
// Resource managers define those endpoints. They will register one or more handlers with the provided APIServer instance accordingly.
ExampleResourceManager exampleResourceManager = new(server, "exampleResource");

Task.WaitAll(Task.Run(async () =>
{
    // Starts the server using the wildcard specifier "+" on port 4650.
    // Note: On Windows, using a wildcard URL for the first time will produce a UAC prompt asking to add the URL to the list of allowed URLs.
    await server.Start("+", 4650);

    // Simple logger that just outputs to the console.
    ConsoleLogger testerLogger = new("Test");

    // Run automated tests to ensure the API behaves as expected.
    await new ExampleApiTester(testerLogger, "localhost", 4650).RunTests();

    // Stops the server.
    server.Stop();
}));