using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using Elements.Core;
using System.Threading;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;

namespace ResoniteApi
{
    internal class ApiEndpoint
    {
        private readonly string _method;
        private readonly Uri _route;

        public ApiEndpoint(string method, string route) : this(method, new Uri(route, UriKind.Relative)) { }

        public ApiEndpoint(string method, Uri route)
        {
            _method = method;
            _route = route;
        }

        public string Method => _method;
        public Uri Route => _route;

        public bool IsMatch(string targetMethod, Uri targetRoute, bool exactMatch)
        {
            if (_method.ToLower() != targetMethod.ToLower()) return false;

            string[] routeSegments = Utils.GetRelativeUriPathSegments(_route);
            string[] targetRouteSegments = Utils.GetRelativeUriPathSegments(targetRoute);
            if (routeSegments.Length != targetRouteSegments.Length) return false;
            for (int i = 0; i < routeSegments.Length; i++)
            {
                if (exactMatch)
                {
                    if (routeSegments[i] != targetRouteSegments[i]) return false;
                }
                else
                {
                    if (routeSegments[i] != targetRouteSegments[i] && !IsPlaceholder(routeSegments[i])) return false;
                }
            }

            return true;
        }

        public string[] GetArgumentsForInputUrl(Uri targetRoute)
        {
            string[] routeSegments = Utils.GetRelativeUriPathSegments(_route);
            string[] targetRouteSegments = Utils.GetRelativeUriPathSegments(targetRoute);

            List<string> arguments = new();

            for (int i = 0; i < routeSegments.Length; i++)
            {
                if (IsPlaceholder(routeSegments[i])) {
                    arguments.Add(targetRouteSegments[i]);
                }
            }

            return arguments.ToArray();
        }

        public static bool IsPlaceholder(string segment)
        {
            bool isPlaceholder = segment.StartsWith("%7B") && segment.EndsWith("%7D");
            UniLog.Log($"[ResoniteApi] IsPlaceholder: '{segment}' -> {isPlaceholder}");
            return isPlaceholder;
        }

        public override string ToString()
        {
            return $"{_method} -> '{_route}'";
        }
    }

    internal struct ApiResponse
    {
        public int statusCode;
        public string? content;

        public ApiResponse(int statusCode, string? content)
        {
            this.statusCode = statusCode;
            this.content = content;
        }
    }

    internal class ApiServer
    {
        private static Dictionary<ApiEndpoint, Func<HttpListenerContext, string[], Task<ApiResponse>>> _handlers = new Dictionary<ApiEndpoint, Func<HttpListenerContext, string[], Task<ApiResponse>>>();
        private HttpListener _listener;
        private bool _isRunning;

        private int? _port;
        private Uri? _baseUri;
        CancellationTokenSource? _listenerCancellationTokenSource;
        Task? _listenerTask;

        public bool IsRunning => _isRunning;
        public int? Port => _port;

        public static void RegisterHandler(ApiEndpoint endpoint, Func<HttpListenerContext, string[], Task<ApiResponse>> handler)
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
                if (endpoint.IsMatch(targetMethod, targetRoute, true)) {
                    return endpoint;
                }
            }

            // No exact match found, check if one with placeholders matches.
            foreach (ApiEndpoint endpoint in _handlers.Keys) {
                if (endpoint.IsMatch(targetMethod, targetRoute, false))
                {
                    return endpoint;
                }
            }

            // No match found.
            return null;
        }

        private static async Task Respond(HttpListenerContext context, ApiResponse response)
        {
            context.Response.StatusCode = response.statusCode;
            context.Response.ContentType = "application/json";
            if (response.content != null)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(response.content);
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }

            context.Response.Close();
        }

        public ApiServer()
        {
            _listener = new HttpListener();
            _isRunning = false;

            // Register default root handler.
            RegisterHandler(new ApiEndpoint("GET", ""), async (HttpListenerContext context, string[] arguments) =>
            {
                string[] endpoints = (from endpoint in _handlers.Keys select endpoint.ToString()).ToArray();
                return new ApiResponse(200, JsonConvert.SerializeObject(endpoints));
            });
        }

        public void Start(int port)
        {
            UniLog.Log($"[ResoniteApi] Starting API Server for port {port}...");
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
            UniLog.Log($"[ResoniteApi] Stopping API Server...");
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
                        string httpMethod = context.Request.HttpMethod;
                        Uri baseUri = _baseUri ?? throw new ArgumentNullException(nameof(_baseUri));
                        Uri relativeRequestUri = baseUri.MakeRelativeUri(context.Request.Url); // TODO: This creates a "negative" relative URI when directly calling baseUri without trailing slash!

                        ApiEndpoint? endpoint = FindBestMatchingEndpoint(httpMethod, relativeRequestUri);
                        if (endpoint != null)
                        {
                            string[] arguments = endpoint.GetArgumentsForInputUrl(relativeRequestUri);
                            response = await _handlers[endpoint].Invoke(context, arguments);
                        }
                        else
                        {
                            string error = $"Unknown endpoint!";
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
