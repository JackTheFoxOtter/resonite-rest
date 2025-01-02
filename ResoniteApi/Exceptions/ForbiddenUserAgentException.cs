using System;

namespace ResoniteApi.Exceptions
{
    internal class ForbiddenUserAgentException : Exception
    {
        private readonly string _userAgent;

        public ForbiddenUserAgentException(string userAgent)
        {
            _userAgent = userAgent;
        }

        public string UserAgent => _userAgent;
    }
}
