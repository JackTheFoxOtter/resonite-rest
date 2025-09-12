using ApiFramework.Enums;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ApiFramework.JSON
{
    internal class JsonValueFilter
    {
        private readonly FilterOperator _filterOperator;
        private readonly IComparable? _comparisonValue;

        JsonValueFilter(FilterOperator filterOperator, string? json) : this(filterOperator, JsonSerializer.Deserialize<JsonValue>(json)) { }
        JsonValueFilter(FilterOperator filterOperator, JsonValue? comparisonValue)
        {
            _filterOperator = filterOperator;
            if (comparisonValue != null)
                _comparisonValue = GetComparableFromJsonValue(comparisonValue);
        }

        public bool Check(JsonValue jsonValue)
        {
            IComparable value = GetComparableFromJsonValue(jsonValue);
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

        public static IComparable GetComparableFromJsonValue(JsonValue jsonValue)
        {
            return jsonValue.GetValueKind() switch
            {
                JsonValueKind.String =>
                    jsonValue.GetValue<string>(),
                JsonValueKind.Number =>
                    jsonValue.GetValue<double>(),
                JsonValueKind.True =>
                    jsonValue.GetValue<bool>(),
                _ =>
                    throw new ArgumentException($"Filtering not defined for JSON value kind '{jsonValue.GetValueKind()}'")
            };
        }

        public static JsonValueFilter FromQueryParamValue(string queryParamValue)
        {
            return queryParamValue switch
            {
                { } when queryParamValue.StartsWith("~null~") => 
                    new JsonValueFilter(FilterOperator.Null, (JsonValue?)null),
                { } when queryParamValue.StartsWith("~notnull~") => 
                    new JsonValueFilter(FilterOperator.NotNull, (JsonValue?)null),
                { } when queryParamValue.StartsWith("~eq~") => 
                    new JsonValueFilter(FilterOperator.Equal, queryParamValue.Substring(4)),
                { } when queryParamValue.StartsWith("~noteq~") => 
                    new JsonValueFilter(FilterOperator.NotEqual, queryParamValue.Substring(7)),
                { } when queryParamValue.StartsWith("~gt~") => 
                    new JsonValueFilter(FilterOperator.GreaterThan, queryParamValue.Substring(4)),
                { } when queryParamValue.StartsWith("~gteq~") => 
                    new JsonValueFilter(FilterOperator.GreaterThanOrEqual, queryParamValue.Substring(6)),
                { } when queryParamValue.StartsWith("~lt~") => 
                    new JsonValueFilter(FilterOperator.LessThan, queryParamValue.Substring(4)),
                { } when queryParamValue.StartsWith("~lteq~") => 
                    new JsonValueFilter(FilterOperator.LessThanOrEqual, queryParamValue.Substring(6)),
                _ => 
                    new JsonValueFilter(FilterOperator.Equal, queryParamValue),
            };
        }
    }
}
