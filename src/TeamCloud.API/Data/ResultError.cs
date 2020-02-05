/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation.Results;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.API.Data
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ResultError
    {
        public ResultErrorCodes Code { get; set; }

        public string Message { get; set; }

        public static ResultError Conflict(string message) => new ResultError
        {
            Code = ResultErrorCodes.Conflict,
            Message = message
        };

        public static ResultError NotFound(string message) => new ResultError
        {
            Code = ResultErrorCodes.NotFound,
            Message = message
        };

        public static ResultError ValidationFailure(ValidationFailure failure) => new ResultError
        {
            Code = ResultErrorCodes.ValidationError,
            Message = $"Validation for '{failure.PropertyName}' failed: {failure.ErrorMessage}"
        };
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResultErrorCodes
    {
        Conflict,
        NotFound,
        ValidationError
    }

    public static class ResultErrorExtensions
    {
    }
}
