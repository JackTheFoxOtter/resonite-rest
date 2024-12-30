using FrooxEngine;
using System;
using System.Net;

namespace ResoniteApi
{
    [Category(new string[] { "Plugins/Resonite Api" })]
    public class ResoniteApi : Component
    {
        public readonly Sync<bool> IsRunning;
        public readonly Sync<int> Port;
        private ApiServer _apiServer;

        public override bool UserspaceOnly => true;

        public ResoniteApi() : base()
        {
            _apiServer = new ApiServer();
        }

        protected override void OnStart()
        {
            base.OnStart();

            IsRunning.Value = false;
            Port.Value = GetDefaultPort();

            ApiServer.RegisterHandler(new ApiEndpoint("GET", "/ping"), async (HttpListenerContext context) =>
            {
                return new ApiResponse(200, "pong");
            });

            if (Port.Value > 0)
            {
                _apiServer.Start(Port.Value);
            }
        }

        protected override void OnCommonUpdate()
        {
            base.OnCommonUpdate();

            IsRunning.Value = _apiServer.IsRunning;
        }

        protected override void OnChanges()
        {
            base.OnChanges();

            if (Port.WasChanged)
            {
                // (Re-)start the API server with new port
                if (_apiServer.IsRunning)
                {
                    _apiServer.Stop();
                }

                if (Port.Value > 0)
                {
                    _apiServer.Start(Port.Value);
                }
            }
        }

        /// <summary>
        /// Retrieves the default port to use for REST-API.
        /// Default value for this is 4600. Can be overridden by passing "--ResoniteApiPort [port]" as launch argument.
        /// </summary>
        /// <returns>
        /// Port number to use as default.
        /// </returns>
        private int GetDefaultPort()
        {
            int port = 4600;

            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                string lower = arg.ToLower();
                bool hasNext = i + 1 < args.Length;

                if (lower.EndsWith("resoniteapiport") && hasNext)
                {
                    string next = args[i + 1];
                    int.TryParse(next, out port);
                    break;
                }
            }

            return port;
        }
    }
}
