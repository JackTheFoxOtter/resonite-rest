using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using Elements.Core;
using System.Threading;
namespace ResoniteApi
{
    internal struct ApiEndpoint
    {
        public string method;
        public Uri route;

        public ApiEndpoint(string method, Uri route)
        {
            this.method = method;
            this.route = route;
        }

        public ApiEndpoint(string method, string route)
        {
            this.method = method;
            this.route = new Uri(route, UriKind.Relative);
        }

        public override string ToString()
        {
            return $"{method} -> {route}";
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
        private static Dictionary<ApiEndpoint, Func<HttpListenerContext, Task<ApiResponse>>> _handlers = new Dictionary<ApiEndpoint, Func<HttpListenerContext, Task<ApiResponse>>>();

        public static void RegisterHandler(ApiEndpoint endpoint, Func<HttpListenerContext, Task<ApiResponse>> handler)
        {
            if (_handlers.ContainsKey(endpoint)) {
                throw new ArgumentException($"A handler is already defined for endpoint {endpoint}!");
            }

            _handlers.Add(endpoint, handler);
        }

        public static void RemoveHandler(ApiEndpoint endpoint)
        {
            if (_handlers.ContainsKey(endpoint))
            {
                _handlers.Remove(endpoint);
            }
        }

        private bool _isRunning;
        HttpListener _listener;

        private int? _port;
        private Uri? _baseUri;
        CancellationTokenSource? _listenerCancellationTokenSource;

        public bool IsRunning => _isRunning;
        public int? Port => _port;

        public ApiServer()
        {
            _isRunning = false;
            _listener = new HttpListener();
        }

        public void Start(int port)
        {
            if (_isRunning)
            {
                throw new ApplicationException("API server is already running!");
            }

            _port = port;
            _baseUri = new Uri($"http://localhost:{port}/ResoniteApi/", UriKind.Absolute);

            // Update prefixes
            _listener.Prefixes.Clear();
            UniLog.Log($"[ResoniteApi] ApiServer: Adding handler for URI '{_baseUri}'");
            _listener.Prefixes.Add(_baseUri.ToString());

            // Start listener
            _listenerCancellationTokenSource = new CancellationTokenSource();
            Task.Run(async () => { 
                await HandleRequests(_listenerCancellationTokenSource.Token); 
            }, _listenerCancellationTokenSource.Token);
            _listener.Start();

            _isRunning = true;
        }

        public void Stop()
        {
            if (!_isRunning)
            {
                throw new ApplicationException("API server is not running!");
            }

            // Stop listener
            _listenerCancellationTokenSource?.Cancel();
            _listener.Stop();

            _port = null;
            _baseUri = null;
            _isRunning = false;
        }
        
        async private Task HandleRequests(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                HttpListenerContext context = await _listener.GetContextAsync();

                cancellationToken.ThrowIfCancellationRequested();

                Uri relativeRequestUri = context.Request.Url.MakeRelativeUri(_baseUri);
                ApiEndpoint endpoint = new ApiEndpoint(context.Request.HttpMethod, relativeRequestUri);
                ApiResponse response;
                if (_handlers.ContainsKey(endpoint))
                {
                    response = await _handlers[endpoint].Invoke(context);
                }
                else
                {
                    response = new ApiResponse(404, "No handler found for route.");
                }
                
                context.Response.StatusCode = response.statusCode;
                context.Response.ContentType = "application/json";
                if (response.content != null) {
                    byte[] buffer = Encoding.UTF8.GetBytes(response.content);
                    await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }

                context.Response.Close();
            }
        }
    }
}
