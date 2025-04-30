using ApiFramework.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiFramework
{
    internal class JValueFilter
    {
        private readonly FilterOperator _filterOperator;
        private readonly JValue? _comparisonValue;

        JValueFilter(FilterOperator filterOperator, JValue? comparisonValue)
        {
            _filterOperator = filterOperator;
            _comparisonValue = comparisonValue;
        }

        JValueFilter(FilterOperator filterOperator, string json)
        {
            _filterOperator = filterOperator;
            _comparisonValue = JsonConvert.DeserializeObject<JValue>(json);
        }

        public bool Check(JValue? value)
        {
            return _filterOperator switch
            {
                FilterOperator.Null => 
                    value == null,
                FilterOperator.NotNull => 
                    value != null,
                FilterOperator.Equal => 
                    value != null && value.CompareTo(_comparisonValue) == 0,
                FilterOperator.NotEqual => 
                    value != null && value.CompareTo(_comparisonValue) != 0,
                FilterOperator.LessThan => 
                    value != null && value.CompareTo(_comparisonValue) < 0,
                FilterOperator.LessThanOrEqual => 
                    value != null && value.CompareTo(_comparisonValue) <= 0,
                FilterOperator.GreaterThan => 
                    value != null && value.CompareTo(_comparisonValue) > 0,
                FilterOperator.GreaterThanOrEqual => 
                    value != null && value.CompareTo(_comparisonValue) >= 0,
                _ => 
                    false
            };
        }

        public static JValueFilter FromQueryParamValue(string queryParamValue)
        {
            return queryParamValue switch
            {
                { } when queryParamValue.StartsWith("~null~") => 
                    new JValueFilter(FilterOperator.Null, (JValue?)null),
                { } when queryParamValue.StartsWith("~notnull~") => 
                    new JValueFilter(FilterOperator.NotNull, (JValue?)null),
                { } when queryParamValue.StartsWith("~eq~") => 
                    new JValueFilter(FilterOperator.Equal, queryParamValue.Substring(4)),
                { } when queryParamValue.StartsWith("~noteq~") => 
                    new JValueFilter(FilterOperator.NotEqual, queryParamValue.Substring(7)),
                { } when queryParamValue.StartsWith("~gt~") => 
                    new JValueFilter(FilterOperator.GreaterThan, queryParamValue.Substring(4)),
                { } when queryParamValue.StartsWith("~gteq~") => 
                    new JValueFilter(FilterOperator.GreaterThanOrEqual, queryParamValue.Substring(6)),
                { } when queryParamValue.StartsWith("~lt~") => 
                    new JValueFilter(FilterOperator.LessThan, queryParamValue.Substring(4)),
                { } when queryParamValue.StartsWith("~lteq~") => 
                    new JValueFilter(FilterOperator.LessThanOrEqual, queryParamValue.Substring(6)),
                _ => 
                    new JValueFilter(FilterOperator.Equal, queryParamValue),
            };
        }
    }
}
