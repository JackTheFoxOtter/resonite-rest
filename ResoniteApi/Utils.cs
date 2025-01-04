using ApiFramework.Exceptions;
using Elements.Core;
using System.Net;

namespace ResoniteApi
{
    internal static class Utils
    {
        /// <summary>
        /// Returns true if the user-agent of the request represents a Resonite client.
        /// Note that this can easily be spoofed! But since this is a userspace plugin, 
        /// we'll assume that our own client doesn't misrepresent the user agent.
        /// </summary>
        /// <param name="request">Request to check the user agent of.</param>
        /// <returns>True if the user agent represents a Resonite client.</returns>
        /// <seealso cref="ThrowIfClientIsResonite(HttpListenerRequest)"/>
        internal static bool IsClientResonite(HttpListenerRequest request)
        {
            return request.UserAgent.StartsWith("Resonite");
        }

        /// <summary>
        /// Throws an exception if the user-agent of the request represents a Resonite client.
        /// Note that this can easily be spoofed! But since this is a userspace plugin, 
        /// we'll assume that our own client doesn't misrepresent the user agent.
        /// </summary>
        /// <param name="request">Request to check the user agent of.</param>
        /// <exception cref="ForbiddenUserAgentException">If the user agent represents a Resonite client.</exception>
        /// <seealso cref="IsClientResonite(HttpListenerRequest)"/>
        internal static void ThrowIfClientIsResonite(HttpListenerRequest request)
        {
            if (IsClientResonite(request))
            {
                throw new ApiForbiddenUserAgentException(request.UserAgent);
            }
        }
    }
}
