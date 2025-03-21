using System.Collections.Specialized;
using System.IO;

using System.Net;
using System.Threading.Tasks;

namespace ApiFramework
{
    public class ApiRequest
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

        public async Task<string?> GetBody()
        {
            if (!_context.Request.HasEntityBody) return null;

            using (Stream body = _context.Request.InputStream)
            {
                using (var reader = new StreamReader(body, _context.Request.ContentEncoding))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}
