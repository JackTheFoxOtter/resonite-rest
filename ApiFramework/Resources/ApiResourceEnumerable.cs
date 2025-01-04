using Elements.Core;
using ApiFramework.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using ApiFramework.Resources;
using Newtonsoft.Json;

namespace ApiFramework
{
    public abstract class ApiResourceEnumerable<T> : IEnumerable<T> where T : ApiResource
    {
        protected abstract IEnumerator<T> Enumerator { get; set; } // TODO: Change to { get; init; } once target framework > 5.0

        public ApiResourceEnumerable(IEnumerable<T> enumerable) 
        {
            Enumerator = enumerable.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Enumerator;
        }

        /// <summary>
        /// Returns a new instance filtered by the query params.
        /// If query params are empty, the new instance will represent the same enumeration as this one.
        /// Currently only supports direct string equality comparison.
        /// If the resource's value is <code>null</code>, it will match to the query param value string <code>~null~</code>.
        /// </summary>
        /// <param name="queryParams">Query parameters to apply filters for.</param>
        /// <returns>Filtered ApiResourceEnumerable<typeparamref name="T"/> instance.</returns>
        /// <exception cref="ApiResourceItemNotFoundException">When the query params contain a key that isn't part of the resource.</exception>
        public ApiResourceEnumerable<T> FilterByQueryParams(NameValueCollection queryParams)
        {
            if (queryParams.Count == 0)
            {
                return (ApiResourceEnumerable<T>) Activator.CreateInstance(GetType(), new object[] { this });
            }

            IEnumerable<T> filtered = this.AsParallel().Where((T resource) =>
            {
                foreach (string? argumentName in queryParams.Keys)
                {
                    if (argumentName == null) continue;

                    if (!resource.ContainsItem(argumentName))
                    {
                        throw new ApiResourceItemNotFoundException(typeof(T), argumentName);
                    }

                    string? resourceValue = resource[argumentName].Value?.ToString();
                    string comparisonValue = queryParams[argumentName];
                    if (resourceValue == null)
                    {
                        if (!comparisonValue.Equals("~null~", StringComparison.InvariantCulture))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!resourceValue.Equals(comparisonValue, StringComparison.InvariantCulture))
                        {
                            return false;
                        }
                    }
                }

                return true;
            });

            return (ApiResourceEnumerable<T>) Activator.CreateInstance(GetType(), new object[] { filtered });
        }

        public IEnumerable<Dictionary<string, object>> GetJsonRepresentation()
        {
            return from resource in this select resource.GetJsonRepresentation();
        }

        public ApiResponse ToResponse()
        {
            return new ApiResponse(200, JsonConvert.SerializeObject(GetJsonRepresentation()));
        }
    }
}
