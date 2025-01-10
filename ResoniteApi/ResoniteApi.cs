using ApiFramework;
using FrooxEngine;
using Newtonsoft.Json;
using ResoniteApi.Resources;

namespace ResoniteApi
{
    [Category(new string[] { "Plugins/Resonite Api" })]
    public class ResoniteApi : Component
    {
        public readonly Sync<bool> IsRunning;
        public readonly Sync<int> Port;
        public readonly Sync<string> Host;
        private ApiServer _apiServer;
        private int _currentPort;
        private string _currentHost;

        public override bool UserspaceOnly => true;

        public ResoniteApi() : base()
        {
            _apiServer = new ApiServer("ResoniteApi");
            _currentPort = 0;
            _currentHost = "";
        }

        protected override void OnStart()
        {
            base.OnStart();

            IsRunning.Value = false;
            Port.Value = GetDefaultPort();
            Host.Value = GetDefaultHost();

            _apiServer.RegisterHandler(new ApiEndpoint("GET", "ping"), async (ApiRequest request) =>
            {
                string response = "pong";

                return new ApiResponse(200, JsonConvert.SerializeObject(response));
            });

            _apiServer.RegisterHandler(new ApiEndpoint("GET", "version"), async (ApiRequest request) =>
            {
                string version = Engine.VersionString;

                return new ApiResponse(200, JsonConvert.SerializeObject(version));
            });

            ContactResourceManager contactManager = new(_apiServer, "contacts", Cloud);
            UserResourceManager userManager = new(_apiServer, "users", Cloud);

            if (Port.Value > 0 && !string.IsNullOrEmpty(Host.Value))
            {
                _currentPort = Port.Value;
                _currentHost = Host.Value;
                Task.Run(async () => await _apiServer.Start(_currentHost, _currentPort));
            }
        }

        protected override void OnCommonUpdate()
        {
            base.OnCommonUpdate();

            if (Port.Value != _currentPort || Host.Value != _currentHost)
            {
                // Port or host was changed, restart API server.
                if (_apiServer.IsRunning)
                {
                    _apiServer.Stop();
                }

                if (Port.Value > 0 && !string.IsNullOrEmpty(Host.Value))
                {
                    _currentPort = Port.Value;
                    _currentHost = Host.Value;
                    Task.Run(async () => await _apiServer.Start(_currentHost, _currentPort));
                }
            }

            IsRunning.Value = _apiServer.IsRunning;
        }

        /// <summary>
        /// Retrieves the default port to use for REST-API.
        /// Default value for this is 4600. Can be overridden by passing "--ResoniteApiPort [port]" as launch argument.
        /// </summary>
        /// <returns>Port number to use as default</returns>
        private int GetDefaultPort()
        {
            int port = 4600;

            string? portArgument = TryGetCommandLineArgValue("resoniteapiport");
            if (portArgument != null) {
                int.TryParse(portArgument, out port);
            }

            return port;
        }

        /// <summary>
        /// Retrieves the default host to use for REST-API.
        /// Default value for this is "localhost". Can be overridden by passing "-ResoniteApiHost [host]" as launch argument.
        /// </summary>
        /// <returns>Hostname to use as default</returns>
        private string GetDefaultHost()
        {
            string host = "localhost";

            string? hostArgument = TryGetCommandLineArgValue("resoniteapihost");
            if (hostArgument != null)
            {
                host = hostArgument;
            }

            return host;
        }

        /// <summary>
        /// Retrieves the value for a given argument name from the command line arguments.
        /// </summary>
        /// <param name="argName">Name of the argument (case insensitive)</param>
        /// <returns>Found value of the argument or null if not found</returns>
        private string? TryGetCommandLineArgValue(string argName)
        {
            string argNameLower = argName.ToLower();
            string? value = null;

            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                string lower = arg.ToLower();
                bool hasNext = i + 1 < args.Length;

                if (lower.EndsWith(argNameLower) && hasNext)
                {
                    value = args[i + 1];
                    break;
                }
            }

            return value;
        }
    }
}
