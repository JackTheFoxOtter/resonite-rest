namespace ApiFramework.Exceptions
{
    public class ApiForbiddenUserAgentException : ApiException
    {
        private readonly string _userAgent;

        public string UserAgent => _userAgent;

        public ApiForbiddenUserAgentException(string userAgent) : base(403)
        {
            _userAgent = userAgent;
        }

        public override string ToString()
        {
            return $"Endpoint forbidden for user agent: '{_userAgent}'";
        }

    }
}
