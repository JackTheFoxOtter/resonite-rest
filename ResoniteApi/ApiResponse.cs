namespace ResoniteApi
{
    internal struct ApiResponse
    {
        private readonly int _statusCode;
        private readonly string? _content;

        public ApiResponse(int statusCode, string? content)
        {
            this._statusCode = statusCode;
            this._content = content;
        }

        public int HttpStatusCode => _statusCode;
        public string? Content => _content;
    }
}
