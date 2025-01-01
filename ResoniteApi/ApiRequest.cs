using System.Collections.Generic;
using System.Net;

namespace ResoniteApi
{
    internal class ApiRequest
    {
        private readonly HttpListenerContext _context;
        private readonly string[] _arguments;
        private readonly Dictionary<string, string> _queryParams;

        public ApiRequest(HttpListenerContext context, string[] arguments, Dictionary<string, string> queryParams)
        {
            _context = context;
            _arguments = arguments;
            _queryParams = queryParams;
        }

        public HttpListenerContext Context => _context;
        public string[] Arguments => _arguments;
        public Dictionary<string, string> QueryParams => _queryParams;
    }
}
