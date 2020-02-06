/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
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

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ValidationError> Errors { get; set; }

        public static ResultError Conflict(string message)
            => new ResultError
            {
                Code = ResultErrorCodes.Conflict,
                Message = message
            };

        public static ResultError NotFound(string message)
            => new ResultError
            {
                Code = ResultErrorCodes.NotFound,
                Message = message
            };

        public static ResultError ValidationFailure(IList<ValidationFailure> failures)
            => new ResultError
            {
                Code = ResultErrorCodes.ValidationError,
                Message = "Validation Failed",
                Errors = failures.Select(f => new ValidationError { Field = f.PropertyName, Message = f.ErrorMessage }).ToList()
            };

        public static ResultError ValidationFailure(ValidationError validationError)
            => new ResultError
            {
                Code = ResultErrorCodes.ValidationError,
                Message = "Validation Failed",
                Errors = new List<ValidationError> { validationError }
            };

        public static ResultError ServerError(Exception exception)
            => new ResultError
            {
                Code = ResultErrorCodes.ServerError,
                Message = $"ServerError: {exception.Message}"
            };
    }

    public class ValidationError
    {
        public string Field { get; set; }

        public string Message { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResultErrorCodes
    {
        Conflict,
        NotFound,
        ServerError,
        ValidationError
    }

    public static class ResultErrorExtensions
    {
    }
}
