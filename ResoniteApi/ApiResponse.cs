namespace ResoniteApi
{
    internal struct ApiResponse
    {
        private readonly int _statusCode;
        private readonly string? _content;

        public ApiResponse(int statusCode, string? content)
        {
            _statusCode = statusCode;
            _content = content;
        }

        public int HttpStatusCode => _statusCode;
        public string? Content => _content;
    }
}
