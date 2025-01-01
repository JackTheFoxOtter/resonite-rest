using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using Elements.Core;
using System.Threading;
using Newtonsoft.Json;
using System.Linq;

namespace ResoniteApi
{
    internal class ApiServer
    {
        private static readonly Dictionary<ApiEndpoint, Func<ApiRequest, Task<ApiResponse>>> _handlers = new();
        private readonly HttpListener _listener;
        private bool _isRunning;

        private int? _port;
        private Uri? _baseUri;
        CancellationTokenSource? _listenerCancellationTokenSource;
        Task? _listenerTask;

        public bool IsRunning => _isRunning;
        public int? Port => _port;

        public static void RegisterHandler(ApiEndpoint endpoint, Func<ApiRequest, Task<ApiResponse>> handler)
        {
            if (_handlers.ContainsKey(endpoint)) {
                throw new ArgumentException($"A handler is already defined for endpoint {endpoint}!");
            }

            UniLog.Log($"[ResoniteApi] Registering handler for endpoint: {endpoint}");
            _handlers.Add(endpoint, handler);
        }

        public static void RemoveHandler(ApiEndpoint endpoint)
        {
            if (_handlers.ContainsKey(endpoint))
            {
                UniLog.Log($"[ResoniteApi] Removing handler for endpoint: {endpoint}");
                _handlers.Remove(endpoint);
            }
        }

        private ApiEndpoint? FindBestMatchingEndpoint(string targetMethod, Uri targetRoute)
        {
            // First check for exact route match.
            foreach (ApiEndpoint endpoint in _handlers.Keys)
            {
                if (endpoint.IsMatchForRequest(targetMethod, targetRoute, true)) {
                    return endpoint;
                }
            }

            // No exact match found, check if one with placeholders matches.
            foreach (ApiEndpoint endpoint in _handlers.Keys) {
                if (endpoint.IsMatchForRequest(targetMethod, targetRoute, false))
                {
                    return endpoint;
                }
            }

            // No match found.
            return null;
        }

        private static async Task Respond(HttpListenerContext context, ApiResponse response)
        {
            context.Response.StatusCode = response.HttpStatusCode;
            context.Response.ContentType = "application/json";
            if (response.Content != null)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(response.Content);
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }

            context.Response.Close();
        }

        public ApiServer()
        {
            _listener = new HttpListener();
            _isRunning = false;

            // Register default root handler.
            RegisterHandler(new ApiEndpoint("GET", ""), async (ApiRequest request) =>
            {
                string[] endpoints = (from endpoint in _handlers.Keys select endpoint.ToString()).ToArray();
                return new ApiResponse(200, JsonConvert.SerializeObject(endpoints));
            });
        }

        public void Start(int port)
        {
            UniLog.Log($"[ResoniteApi] Starting API server for port {port}...");
            if (_isRunning)
            {
                throw new ApplicationException("API server is already running!");
            }

            _port = port;
            _baseUri = new Uri($"http://localhost:{port}/ResoniteApi/", UriKind.Absolute);
            UniLog.Log($"[ResoniteApi] ApiServer: Setting handler base URI to '{_baseUri}'");

            // Update prefixes.
            _listener.Prefixes.Clear();
            _listener.Prefixes.Add(_baseUri.ToString());

            // Start listener.
            _listener.Start();
            _listenerCancellationTokenSource = new CancellationTokenSource();
            _listenerTask = Task.Run(async () => { 
                await HandleRequests(_listenerCancellationTokenSource.Token);
            }, _listenerCancellationTokenSource.Token);

            _isRunning = true;
        }

        public void Stop()
        {
            UniLog.Log($"[ResoniteApi] Stopping API server...");
            if (!_isRunning)
            {
                throw new ApplicationException("API server is not running!");
            }

            // Stop listener.
            _listener.Stop();
            _listenerCancellationTokenSource?.Cancel();
            _listenerTask?.Wait();

            _port = null;
            _baseUri = null;
            _listenerCancellationTokenSource = null;
            _listenerTask = null;

            _isRunning = false;
        }
        
        async private Task HandleRequests(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    HttpListenerContext context = await _listener.GetContextAsync().WrapCancellable(cancellationToken);
                    UniLog.Log($"[ResoniteApi] Received request: {context.Request.Url}, UserAgent: {context.Request.UserAgent}");

                    ApiResponse response = new ApiResponse(500, JsonConvert.SerializeObject("Something went wrong!"));
                    try
                    {
                        // Ensure base Uri is defined
                        string httpMethod = context.Request.HttpMethod;
                        Uri baseUri = _baseUri ?? throw new ArgumentNullException(nameof(_baseUri));
                        
                        // Ensure the request Uri ends with a trailing slash before computing relative route Uri (otherwise it doesn't behave as expected)
                        string requestUriString = context.Request.Url.GetComponents(UriComponents.AbsoluteUri, UriFormat.UriEscaped);
                        Uri requestUri = requestUriString.EndsWith("/") ? new(requestUriString, UriKind.Absolute) : new(requestUriString + "/", UriKind.Absolute);
                        
                        // Compute API route (relative Uri from base to request)
                        Uri apiRoute = baseUri.MakeRelativeUri(requestUri);

                        ApiEndpoint? endpoint = FindBestMatchingEndpoint(httpMethod, apiRoute);
                        if (endpoint != null)
                        {
                            string[] arguments = endpoint.ParseRequestArguments(httpMethod, apiRoute);
                            Dictionary<string, string> queryParams = new();
                            ApiRequest request = new(context, arguments, queryParams);
                            response = await _handlers[endpoint].Invoke(request);
                        }
                        else
                        {
                            string error = $"No endpoint found for route: '{apiRoute}'";
                            response = new ApiResponse(404, JsonConvert.SerializeObject(error));
                        }
                    }
                    catch (ForbiddenUserAgentException ex)
                    {
                        string error = $"Endpoint forbidden for user agent: '{ex.UserAgent}'";
                        response = new ApiResponse(403, JsonConvert.SerializeObject(error));
                    }
                    catch (Exception ex)
                    {
                        string error = $"Exception occured while processing request: {ex}";
                        response = new ApiResponse(500, JsonConvert.SerializeObject(error));
                        
                        throw ex;
                    }
                    finally
                    {
                        await Respond(context, response);
                    }
                }
                catch (TaskCanceledException)
                {
                    // Task was cancelled.
                    break;
                }
                catch (Exception ex)
                {
                    // Something went wrong.
                    UniLog.Log($"[ResoniteApi] Exception during handling of request!\n{ex}");
                }
            }
        }
        
    }
}
