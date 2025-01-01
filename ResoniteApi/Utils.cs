using Elements.Core;
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
        internal static bool IsClientResonite(HttpListenerRequest request)
        {
            return request.UserAgent.StartsWith("Resonite");
        }

        internal static void ThrowIfClientIsResonite(HttpListenerRequest request)
        {
            if (IsClientResonite(request))
            {
                throw new ForbiddenUserAgentException(request.UserAgent);
            }
        }

        internal static string[] GetRelativeUriPathSegments(Uri relativeUri)
        {
            Uri fakeRoot = new Uri("http://host", UriKind.Absolute);
            Uri fakeRooted = new Uri(fakeRoot, relativeUri);
            string[] segments = (from segment in fakeRooted.Segments select segment.TrimEnd('/')).ToArray();

            string segmentsStr = string.Join(", ", segments);
            UniLog.Log($"[ResoniteApi] GetRelativeUriSegments: '{relativeUri}' -> '{segmentsStr}' ({segments.Length} segments)");

            return segments;
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
