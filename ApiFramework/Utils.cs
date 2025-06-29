﻿using ApiFramework.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ApiFramework
{
    public static class Utils
    {
        /// <summary>
        /// Parses a query parameter value as JSON, returns it and removes it from the query parameters.
        /// </summary>
        /// <typeparam name="T">Target type of desired parameter</typeparam>
        /// <param name="queryParams">Query parameters</param>
        /// <param name="paramName">Name fo desired parameter</param>
        /// <returns>The parsed parameter value in the target type</returns>
        /// <exception cref="ArgumentException"></exception>
        public static T? PopJsonParam<T>(this NameValueCollection queryParams, string paramName)
        {
            T? paramValue = default;

            if (queryParams.AllKeys.Contains(paramName))
            {
                string paramValueJson = queryParams.Get(paramName);
                queryParams.Remove(paramName);

                try
                {
                    paramValue = JsonConvert.DeserializeObject<T>(paramValueJson);
                }
                catch (JsonReaderException ex)
                {
                    throw new ApiJsonParsingException($"Failed to parse query parameter '{paramName}'. (Expected JSON-formatted {typeof(T)} value)");
                }

            }
            
            return paramValue;
        }

        /// <summary>
        /// Returns a nicely formatted type name of the target type.
        /// </summary>
        /// <param name="type">Type to get type name for.</param>
        /// <returns>Nicely formatted type name for target type.</returns>
        public static string GetNiceTypeName(this Type type)
        {
            if (type.IsGenericType && type.Name.Contains('`'))
            {
                StringBuilder builder = new();
                builder.Append(type.Name.Split('`')[0]);
                builder.Append("<");
                Type[] genericArguments = type.GetGenericArguments();
                for (int i = 0; i < genericArguments.Length; i++)
                {
                    Type argumentType = genericArguments[i];
                    builder.Append(argumentType.GetNiceTypeName());
                    if (i < genericArguments.Length - 1) 
                        builder.Append(", ");
                }
                builder.Append(">");
                return builder.ToString();
            }
            return type.Name;
        }

        /// <summary>
        /// Helper function to retrieve the path segments of a relative Uri, by converting it to a 'fake' absolute Uri first.
        /// </summary>
        /// <param name="relativeUri">Relative Uri to retrieve path segments from</param>
        /// <returns>Array of path segment strings</returns>
        public static string[] GetRelativeUriPathSegments(Uri relativeUri)
        {
            Uri fakeRoot = new("http://host", UriKind.Absolute);
            Uri fakeRooted = new(fakeRoot, relativeUri);
            string[] segments = (from segment in fakeRooted.Segments select segment.TrimEnd('/')).ToArray();

            return segments;
        }

        /// <summary>
        /// Returns true if the input string matches the placeholder syntax. (starts with '{' & ends with '}')
        /// Checks for both unescaped and URI-escaped variants.
        /// </summary>
        /// <param name="segment">Segment string to check (usually segment of request URI)</param>
        /// <returns>True if the segment follows the placeholder syntax</returns>
        public static bool IsPlaceholder(string segment)
        {
            return (segment.StartsWith("{") || segment.StartsWith("%7B")) && (segment.EndsWith("}") || segment.EndsWith("%7D"));
        }

        /// <summary>
        /// Returns true if the input string matches the 'greedy placeholder' syntax. ('{...}')
        /// Checks for both unescaped and URI-escaped variants.
        /// </summary>
        /// <param name="segment">Segment string to check (usually segment of request URI)</param>
        /// <returns>True if the segment follows the 'greedy placeholder' syntax</returns>
        public static bool IsGreedyPlaceholder(string segment)
        {
            return segment.Equals("{...}") || segment.Equals("%7B...%7D");
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

        /// <summary>
        /// For using wildcard HTTP listener on Windows, the listener address needs to be added to the HTTP access control list (acl).
        /// Attention: This opens a UAC dialog when the address isn't already allowed!
        /// </summary>
        /// <param name="address">Target address to add to the acl list</param>
        public static async Task AddAclAddress(string address)
        {
            string args = string.Format(@"http add urlacl url={0} user={1}\{2}", address, Environment.UserDomainName, Environment.UserName);

            ProcessStartInfo psi = new("netsh", args)
            {
                Verb = "runas",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true
            };

            await Process.Start(psi).WaitForExitAsync();
        }

        /// <summary>
        /// Wait for a process to exit asynchronously.
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public static async Task WaitForExitAsync(this Process process)
        {
            TaskCompletionSource<bool> completion = new();

            process.Exited += (object sender, EventArgs e) => completion.SetResult(true);
            if (process.HasExited) completion.TrySetResult(true); // In case it exited before the event handler was registered

            await completion.Task;
        }
    }
}
