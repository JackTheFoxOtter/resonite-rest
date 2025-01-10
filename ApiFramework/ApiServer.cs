using ApiFramework.Exceptions;
using Elements.Core;
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
    public class ApiServer
    {
        private readonly Dictionary<ApiEndpoint, Func<ApiRequest, Task<ApiResponse>>> _handlers = new();
        private string _baseRoute;
        private bool _isRunning = false;

        private CancellationTokenSource? _listenerCancellationTokenSource;
        private HttpListener? _listener;
        private Task? _listenerTask;

        public bool IsRunning => _isRunning;

        public void RegisterHandler(ApiEndpoint endpoint, Func<ApiRequest, Task<ApiResponse>> handler)
        {
            if (_handlers.ContainsKey(endpoint)) {
                throw new ArgumentException($"A handler is already defined for endpoint {endpoint}!");
            }

            UniLog.Log($"[ResoniteApi] Registering handler for endpoint: {endpoint}");
            _handlers.Add(endpoint, handler);
        }

        public void RemoveHandler(ApiEndpoint endpoint)
        {
            if (_handlers.ContainsKey(endpoint))
            {
                UniLog.Log($"[ResoniteApi] Removing handler for endpoint: {endpoint}");
                _handlers.Remove(endpoint);
            }
        }

        private ApiEndpoint? FindBestMatchingEndpoint(string targetMethod, Uri targetRoute)
        {
            foreach (ApiEndpoint endpoint in _handlers.Keys)
            {
                if (endpoint.IsMatchForRequest(targetMethod, targetRoute, true)) {
                    return endpoint;
                }
            }

            foreach (ApiEndpoint endpoint in _handlers.Keys) {
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

        public ApiServer(string? baseRoute)
        {
            if (baseRoute != null && !string.IsNullOrEmpty(baseRoute))
            {
                _baseRoute = baseRoute.Trim('/');
            }
            else
            {
                _baseRoute = "";
            }

            // Register default root handler.
            RegisterHandler(new ApiEndpoint("GET", ""), async (ApiRequest request) =>
            {
                string[] endpoints = (from endpoint in _handlers.Keys select endpoint.ToString()).ToArray();
                return new ApiResponse(200, JsonConvert.SerializeObject(endpoints));
            });
        }

        public async Task Start(string host, int port)
        {
            if (_isRunning) throw new ApplicationException("API server is already running!");

            UniLog.Log($"[ResoniteApi] Starting API server...");

            string uriPrefix = $"http://{host}:{port}/{_baseRoute}/";
            try
            {
                // Attempt to start the listener
                StartInternal(uriPrefix);
            }
            catch (HttpListenerException e)
            {
                if (e.ErrorCode != 5) throw;
                
                // Access denied, this indicates that the requested URI can't be listened on (probably because it's a wildcard one)
                // Add URI to HTTP access control list for current user and retry.
                UniLog.Log("[ResoniteApi] Access denied, requesting to add URI prefix to HTTP access control list...");
                await Utils.AddAclAddress(uriPrefix);
                StartInternal(uriPrefix);
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

            _isRunning = true;
            
            UniLog.Log($"[ResoniteApi] Listening on: {uriPrefix}");
        }

        public void Stop()
        {
            UniLog.Log($"[ResoniteApi] Stopping API server...");
            if (!_isRunning)
            {
                throw new ApplicationException("API server is not running!");
            }

            // Stop listener.
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
            _isRunning = false;
        }
        
        async private Task HandleRequests(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _listener != null)
            {
                try
                {
                    HttpListenerContext context = await _listener.GetContextAsync().WrapCancellable(cancellationToken);
                    UniLog.Log($"[ResoniteApi] Received request: '{context.Request.HttpMethod} {context.Request.Url}', UserAgent: '{context.Request.UserAgent}'");

                    ApiResponse? response = null;
                    try
                    {
                        // Determine API route of request
                        string httpMethod = context.Request.HttpMethod;
                        string requestPath = context.Request.Url.AbsolutePath.Trim('/');
                        if (requestPath.StartsWith(_baseRoute)) requestPath = requestPath.Remove(0, _baseRoute.Length);
                        Uri apiRoute = new(requestPath, UriKind.Relative);

                        ApiEndpoint? endpoint = FindBestMatchingEndpoint(httpMethod, apiRoute);
                        if (endpoint != null)
                        {
                            string[] arguments = endpoint.ParseRequestArguments(httpMethod, apiRoute);
                            NameValueCollection queryParams = HttpUtility.ParseQueryString(context.Request.Url.Query);
                            ApiRequest request = new(context, arguments, queryParams);
                            response = await _handlers[endpoint].Invoke(request);
                        }
                        else
                        {
                            string error = $"No endpoint found for route: '{apiRoute}'";
                            response = new ApiResponse(404, JsonConvert.SerializeObject(error));
                        }
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
                            string error = $"One or more unhandled exception while processing request: {aggregateEx}";
                            response = new ApiResponse(500, JsonConvert.SerializeObject(error));
                            throw;
                        }
                    }
                    catch (Exception ex)
                    {
                        string error = $"Unhandled exception while processing request: {ex}";
                        response = new ApiResponse(500, JsonConvert.SerializeObject(error));
                        throw;
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
