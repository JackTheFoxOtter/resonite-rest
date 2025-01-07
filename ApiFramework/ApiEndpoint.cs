using System;
using System.Collections.Generic;

namespace ApiFramework
{
    public class ApiEndpoint
    {
        private readonly string _httpMethod;
        private readonly Uri _route;

        public ApiEndpoint(string method, string route) : this(method, new Uri(route, UriKind.Relative)) { }

        public ApiEndpoint(string method, Uri route)
        {
            _httpMethod = method;
            _route = route;
        }

        public string Method => _httpMethod;
        public Uri Route => _route;

        /// <summary>
        /// Checks wether this endpoint is a match for the specified route & httpMethod.
        /// If exactMatch is true, this function will return true if
        /// - The httpMethod of the endpoint matches the httpMethod of the target route
        /// - All segments of the endpoint match the corresponding segments in the target route exactly
        /// If exactMath is false, this function will return true if
        /// - The httpMethod of the endpoint matches the httpMethod of the target route
        /// - All **non-placeholder** segments of the endpoint match the corresponding segments in the target route exactly
        /// </summary>
        /// <param name="targetHttpMethod">HttpMethod of the target request</param>
        /// <param name="targetRoute">Route of the target request</param>
        /// <param name="exactMatch">Whether to use exact matching only or also consider placeholders in the endpoint route.</param>
        /// <returns>True if this endpoint is a match for the target request.</returns>
        public bool IsMatchForRequest(string targetHttpMethod, Uri targetRoute, bool exactMatch)
        {
            if (_httpMethod.ToLower() != targetHttpMethod.ToLower()) return false;

            string[] endpointRouteSegments = Utils.GetRelativeUriPathSegments(_route);
            string[] targetRouteSegments = Utils.GetRelativeUriPathSegments(targetRoute);

            if (!exactMatch && Utils.IsGreedyPlaceholder(endpointRouteSegments[endpointRouteSegments.Length - 1]))
            {
                // Greedy placeholder matches n number of elements, so any length >= its index could match
                if (targetRouteSegments.Length < endpointRouteSegments.Length - 1) return false;
            }
            else
            {
                // Otherwise matches have to be of equal length
                if (targetRouteSegments.Length != endpointRouteSegments.Length) return false;
            }
            
            for (int i = 0; i < endpointRouteSegments.Length; i++)
            {
                if (exactMatch)
                {
                    if (endpointRouteSegments[i] != targetRouteSegments[i]) return false;
                }
                else
                {
                    if (Utils.IsGreedyPlaceholder(endpointRouteSegments[i])) return true; // Greedy placeholder matches n number of elements, so we can skip checking further
                    if (endpointRouteSegments[i] != targetRouteSegments[i] && !Utils.IsPlaceholder(endpointRouteSegments[i])) return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns an array of all segments in the target request route that correspond to a placeholder segment in this endpoint's route.
        /// </summary>
        /// <exception cref="ArgumentException">When the target request isn't matching this endpoint. (See IsMatchForRequest)</exception>
        /// <param name="targetRoute"></param>
        /// <returns></returns>
        /// <see cref="IsMatchForRequest(string, Uri, bool)"/>
        public string[] ParseRequestArguments(string targetHttpMethod, Uri targetRoute)
        {
            if (!IsMatchForRequest(targetHttpMethod, targetRoute, false))
            {
                throw new ArgumentException("The target request doesn't match this endpoint, so no arguments can be parsed!");
            }

            string[] endpointRouteSegments = Utils.GetRelativeUriPathSegments(_route);
            string[] targetRouteSegments = Utils.GetRelativeUriPathSegments(targetRoute);

            List<string> arguments = new();

            bool greedyFlag = false;
            for (int i = 0; i < targetRouteSegments.Length; i++)
            {
                if (greedyFlag || Utils.IsPlaceholder(endpointRouteSegments[i]))
                {
                    arguments.Add(targetRouteSegments[i]);
                    if (!greedyFlag && Utils.IsGreedyPlaceholder(endpointRouteSegments[i])) greedyFlag = true;
                }
            }

            return arguments.ToArray();
        }

        public override string ToString()
        {
            return $"{_httpMethod} {_route}";
        }
    }
}
