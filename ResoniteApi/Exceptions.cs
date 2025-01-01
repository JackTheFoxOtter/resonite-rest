using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResoniteApi
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

    internal class ApiValueReadOnlyException : Exception
    {
        private readonly string _valueName;

        public ApiValueReadOnlyException(string valueName) 
        {
            _valueName = valueName;
        }

        public string ValueName => _valueName;
    }

}
