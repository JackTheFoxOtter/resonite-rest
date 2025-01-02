using System.Collections.Specialized;
using System.Net;

namespace ResoniteApi
{
    internal class ApiRequest
    {
        private readonly HttpListenerContext _context;
        private readonly string[] _arguments;
        private readonly NameValueCollection _queryParams;

        public ApiRequest(HttpListenerContext context, string[] arguments, NameValueCollection queryParams)
        {
            _context = context;
            _arguments = arguments;
            _queryParams = queryParams;
        }

        public HttpListenerContext Context => _context;
        public string[] Arguments => _arguments;
        public NameValueCollection QueryParams => _queryParams;
    }
}
