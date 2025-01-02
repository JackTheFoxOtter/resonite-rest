using Elements.Core;
using ResoniteApi.Exceptions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace ResoniteApi
{
    internal abstract class ApiResourceEnumerable<T> : IEnumerable<T> where T : ApiResource
    {
        protected abstract IEnumerator<T> Enumerator { get; set; } // TODO: Change to { get; init; } once target framework > 5.0

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Enumerator;
        }

        public void FilterByQueryParams(NameValueCollection queryParams)
        {
            Enumerator = this.AsParallel().Where((T resource) =>
            {
                foreach (string key in queryParams)
                {
                    if (!resource.ContainsItem(key))
                    {
                        throw new ApiQueryException($"ApiResource '{resource}' doesn't contain ApiItem with name '{key}'!");
                    }

                    string resourceValue = resource[key].Value.ToString();
                    string comparisonValue = queryParams[key];
                    UniLog.Log($"Key: {key}, Value: {resourceValue}, Compare against: {comparisonValue}, Equal: {resourceValue.Equals(comparisonValue, System.StringComparison.InvariantCultureIgnoreCase)}");
                    if (!resourceValue.Equals(comparisonValue, System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        return false;
                    }
                }

                return true;
            }).GetEnumerator();
        }

        public IEnumerable<Dictionary<string, object>> GetJsonRepresentation()
        {
            return from resource in this select resource.GetJsonRepresentation();
        }
    }
}
