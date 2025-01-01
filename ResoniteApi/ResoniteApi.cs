using Newtonsoft.Json;
using System;
using System.Net;

using FrooxEngine;
using SkyFrost;
using System.CodeDom;
using SkyFrost.Base;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using Elements.Core;

namespace ResoniteApi
{
    [Category(new string[] { "Plugins/Resonite Api" })]
    public class ResoniteApi : Component
    {
        public readonly Sync<bool> IsRunning;
        public readonly Sync<int> Port;
        private ApiServer _apiServer;
        private int _currentPort;

        public override bool UserspaceOnly => true;

        public ResoniteApi() : base()
        {
            _apiServer = new ApiServer();
            _currentPort = 0;
        }

        protected override void OnStart()
        {
            base.OnStart();

            IsRunning.Value = false;
            Port.Value = GetDefaultPort();

            ApiServer.RegisterHandler(new ApiEndpoint("GET", "ping"), async (HttpListenerContext context, string[] arguments) =>
            {
                string response = "pong";

                return new ApiResponse(200, JsonConvert.SerializeObject(response));
            });

            ApiServer.RegisterHandler(new ApiEndpoint("GET", "version"), async (HttpListenerContext context, string[] arguments) =>
            {
                string version = Engine.VersionString;

                return new ApiResponse(200, JsonConvert.SerializeObject(version));
            });

            ApiServer.RegisterHandler(new ApiEndpoint("GET", "contacts"), async (HttpListenerContext context, string[] arguments) =>
            {
                Utils.ThrowIfClientIsResonite(context.Request); // Don't allow from within Resonite

                ContactResourceEnumerable contacts = new((await Cloud.Contacts.GetContacts()).Entity);

                return new ApiResponse(200, JsonConvert.SerializeObject(contacts.GetJsonRepresentation()));
            });

            ApiServer.RegisterHandler(new ApiEndpoint("GET", "contacts/{contactUserId}"), async (HttpListenerContext context, string[] arguments) =>
            {
                Utils.ThrowIfClientIsResonite(context.Request); // Don't allow from within Resonite

                string contactUserId = arguments[0];
                ContactResource contact = new(Cloud.Contacts.GetContact(contactUserId));

                return new ApiResponse(200, JsonConvert.SerializeObject(contact.GetJsonRepresentation()));
            });

            if (Port.Value > 0)
            {
                _currentPort = Port.Value;
                _apiServer.Start(Port.Value);
            }
        }

        protected override void OnCommonUpdate()
        {
            base.OnCommonUpdate();

            if (Port.Value != _currentPort)
            {
                // Port was changed, restart API server on new port.
                if (_apiServer.IsRunning)
                {
                    _apiServer.Stop();
                }

                if (Port.Value > 0)
                {
                    _currentPort = Port.Value;
                    _apiServer.Start(Port.Value);
                }
            }

            IsRunning.Value = _apiServer.IsRunning;
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
