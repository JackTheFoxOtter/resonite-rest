using ApiFramework.Exceptions;
using ApiFramework.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ApiFramework
{
    /// <summary>
    /// Hosts an HTTP API.
    /// Allows registering handlers for routes and automatically determines which handler to invoke for incoming requests.
    /// </summary>
    public class ApiServer
    {
        private CancellationTokenSource? _listenerCancellationTokenSource;
        private HttpListener? _listener;
        private Task? _listenerTask;

        private Dictionary<ApiEndpoint, Func<ApiRequest, Task<ApiResponse>>> Handlers { get; } = new();
        public ILogger Logger { get; }
        public string BaseRoute { get; }
        public bool IsRunning { get; private set; }

        public ApiServer(ILogger logger, string? baseRoute)
        {
            Logger = logger;
            BaseRoute = (baseRoute != null && baseRoute.Length > 0) ? baseRoute.Trim('/') : String.Empty;

            // Register default root handler.
            RegisterHandler(new ApiEndpoint("GET", String.Empty), async (ApiRequest request) =>
            {
                // List all registered endpoints.
                string[] endpoints = (from endpoint in Handlers.Keys select endpoint.ToString()).ToArray();
                return new ApiResponse(200, JsonConvert.SerializeObject(endpoints));
            });
        }

        public void RegisterHandler(ApiEndpoint endpoint, Func<ApiRequest, Task<ApiResponse>> handler)
        {
            if (Handlers.ContainsKey(endpoint)) {
                throw new ArgumentException($"A handler is already defined for endpoint {endpoint}!");
            }

            Logger.Log($"Registering handler for endpoint: {endpoint}");
            Handlers.Add(endpoint, handler);
        }

        public void RemoveHandler(ApiEndpoint endpoint)
        {
            if (Handlers.ContainsKey(endpoint))
            {
                Logger.Log($"Removing handler for endpoint: {endpoint}");
                Handlers.Remove(endpoint);
            }
        }

        private ApiEndpoint? FindBestMatchingEndpoint(string targetMethod, Uri targetRoute)
        {
            foreach (ApiEndpoint endpoint in Handlers.Keys)
            {
                if (endpoint.IsMatchForRequest(targetMethod, targetRoute, true)) {
                    return endpoint;
                }
            }

            foreach (ApiEndpoint endpoint in Handlers.Keys) {
                if (endpoint.IsMatchForRequest(targetMethod, targetRoute, false))
                {
                    return endpoint;
                }
            }

            return null;
        }

        private static async Task Respond(HttpListenerContext context, ApiResponse? response)
        {
            if (response != null)
            {
                context.Response.StatusCode = response.HttpStatusCode;
                context.Response.ContentType = "application/json";
                if (response.Content != null)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(response.Content);
                    await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
            }

            context.Response.Close();
        }

        public async Task Start(string host, int port)
        {
            if (IsRunning) throw new ApplicationException("API server is already running!");

            string uriPrefix = $"http://{host}:{port}/{BaseRoute}/";
            
            Logger.Log($"Attempting to start API server...");
            try
            {
                // Attempt to start the listener.
                StartInternal(uriPrefix);
            }
            catch (HttpListenerException e)
            {
                if (e.ErrorCode != 5) throw;

                // Access denied, this indicates that the requested URI can't be listened on (probably because it's a wildcard one).
                // Add URI to HTTP access control list for current user and retry.
                Logger.Log("Access denied, requesting to add URI prefix to HTTP access control list...");
                await Utils.AddAclAddress(uriPrefix);
                StartInternal(uriPrefix);
            }
            finally 
            {
                if (IsRunning)
                {
                    Logger.Log($"API server started & listening on: {uriPrefix}");
                }
                else
                {
                    throw new ApplicationException("Failed to start API server!");
                }
            }
        }

        private void StartInternal(string uriPrefix)
        {
            _listenerCancellationTokenSource = new CancellationTokenSource();
            _listener = new HttpListener();
            _listener.Prefixes.Add(uriPrefix);

            _listener.Start();
            _listenerTask = Task.Run(async () => {
                await HandleRequests(_listenerCancellationTokenSource.Token);
            }, _listenerCancellationTokenSource.Token);

            IsRunning = true;
        }

        public void Stop()
        {
            if (!IsRunning) throw new ApplicationException("API server is not running!");

            Logger.Log($"Stopping API server...");

            StopInternal();
        }

        private void StopInternal()
        {
            _listener?.Stop();
            _listenerCancellationTokenSource?.Cancel();
            _listenerTask?.Wait();

            _listenerCancellationTokenSource = null;
            _listener = null;
            _listenerTask = null;
            IsRunning = false;
        }
        
        async private Task HandleRequests(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _listener != null)
            {
                // First wait for incoming request.
                HttpListenerContext context;
                try
                {
                    context = await _listener.GetContextAsync().WrapCancellable(cancellationToken);
                    Logger.Log($"Received request: '{context.Request.HttpMethod} {context.Request.Url}', UserAgent: '{context.Request.UserAgent}'");
                }
                catch (TaskCanceledException)
                {
                    // Task was cancelled.
                    break;
                }

                // Then handle the request & respond.
                ApiResponse? response = null;
                try
                {
                    response = await HandleRequest(context);
                }
                catch (ApiException apiEx)
                {
                    response = apiEx.ToResponse();
                }
                catch (AggregateException aggregateEx)
                {
                    foreach (Exception innerEx in aggregateEx.Flatten().InnerExceptions)
                    {
                        if (innerEx is ApiException apiEx)
                        {
                            response = apiEx.ToResponse();
                            break;
                        }
                    }

                    if (response == null)
                    {
                        string error = $"One or more unhandled exceptions occured while processing request: {aggregateEx}";
                        response = new ApiResponse(500, JsonConvert.SerializeObject(error));
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    string error = $"Unhandled exception occured while processing request: {ex}";
                    response = new ApiResponse(500, JsonConvert.SerializeObject(error));
                    throw;
                }
                finally
                {
                    if (response == null)
                    {
                        // This should't be possible.
                        string error = $"Failed to determine suitable response for request!";
                        response = new ApiResponse(500, JsonConvert.SerializeObject(error));
                    }
                    
                    await Respond(context, response);
                }
            }
        }

        async private Task<ApiResponse> HandleRequest(HttpListenerContext context)
        {
            // Determine API route of request.
            string httpMethod = context.Request.HttpMethod;
            string requestPath = context.Request.Url.AbsolutePath.Trim('/');
            if (requestPath.StartsWith(BaseRoute)) requestPath = requestPath.Remove(0, BaseRoute.Length);
            Uri apiRoute = new(requestPath, UriKind.Relative);

            // Try to find & invoke endpoint to handle request.
            ApiEndpoint? endpoint = FindBestMatchingEndpoint(httpMethod, apiRoute);
            if (endpoint != null)
            {
                string[] arguments = endpoint.ParseRequestArguments(httpMethod, apiRoute);
                NameValueCollection queryParams = HttpUtility.ParseQueryString(context.Request.Url.Query);
                ApiRequest request = new(context, arguments, queryParams);
                return await Handlers[endpoint].Invoke(request);
            }
            else
            {
                string error = $"No endpoint found for route: '{apiRoute}'";
                return new ApiResponse(404, JsonConvert.SerializeObject(error));
            }
        }
    }
}
