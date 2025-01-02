using Elements.Core;
using ResoniteApi.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
                throw new ForbiddenUserAgentException(request.UserAgent);
            }
        }

        /// <summary>
        /// Helper function to retrieve the path segments of a relative Uri, by converting it to a 'fake' absolute Uri first.
        /// </summary>
        /// <param name="relativeUri">Relative Uri to retrieve path segments from.</param>
        /// <returns>Array of path segment strings.</returns>
        internal static string[] GetRelativeUriPathSegments(Uri relativeUri)
        {
            Uri fakeRoot = new("http://host", UriKind.Absolute);
            Uri fakeRooted = new(fakeRoot, relativeUri);
            string[] segments = (from segment in fakeRooted.Segments select segment.TrimEnd('/')).ToArray();

            string segmentsStr = string.Join(", ", segments);
            UniLog.Log($"[ResoniteApi] GetRelativeUriSegments: '{relativeUri}' -> '{segmentsStr}' ({segments.Length} segments)");

            return segments;
        }
        
        /// <summary>
        /// Returns true if the input string matches the placeholder syntax. (starts with '{' & ends with '}')
        /// Checks for both unescaped and URI-escaped variants.
        /// </summary>
        /// <param name="segment">Segment string to check (usually segment of request URI)</param>
        /// <returns>True if the segment follows the placeholder syntax.</returns>
        internal static bool IsPlaceholder(string segment)
        {
            bool isPlaceholder = (segment.StartsWith("{") || segment.StartsWith("%7B")) && (segment.EndsWith("}") || segment.EndsWith("%7D"));
            return isPlaceholder;
        }

        /// <summary>
        /// Wraps an async task to allow for cancellation via token.
        /// Based on https://stackoverflow.com/a/69861689/12819187
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        internal static Task<T> WrapCancellable<T>(this Task<T> task, CancellationToken token)
        {
            if (!token.CanBeCanceled)
            {
                return task;
            }

            var tcs = new TaskCompletionSource<T>();
            // This cancels the returned task:
            // 1. If the token has been canceled, it cancels the TCS straightaway
            // 2. Otherwise, it attempts to cancel the TCS whenever
            //    the token indicates cancelled
            token.Register(() => tcs.TrySetCanceled(token), useSynchronizationContext: false);

            task.ContinueWith(t => 
            {
                // Complete the TCS per task status
                // If the TCS has been cancelled, this continuation does nothing
                if (task.IsCanceled)
                {
                    tcs.TrySetCanceled();
                }
                else if (task.IsFaulted)
                {
                    tcs.TrySetException(t.Exception);
                }
                else
                {
                    tcs.TrySetResult(t.Result);
                }
            },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default
            );

            return tcs.Task;
        }
    }
}
