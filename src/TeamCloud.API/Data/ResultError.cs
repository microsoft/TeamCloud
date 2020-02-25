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


        public static ResultError Failed(Exception exception)
            => new ResultError
            {
                Code = ResultErrorCodes.Failed,
                Message = $"Operation Failed: {exception.Message}"
            };

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

        public static ResultError Unauthorized()
            => new ResultError
            {
                Code = ResultErrorCodes.Unauthorized,
                Message = "Unauthorized"
            };

        public static ResultError Forbidden()
            => new ResultError
            {
                Code = ResultErrorCodes.Forbidden,
                Message = "Forbidden"
            };

        public static ResultError ServerError(Exception exception)
            => new ResultError
            {
                Code = ResultErrorCodes.ServerError,
                Message = $"ServerError: {exception.Message}"
            };

        public static ResultError Unknown()
            => new ResultError
            {
                Code = ResultErrorCodes.Unknown,
                Message = "An unknown error occured."
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
        Unknown,
        Failed,
        Conflict,
        NotFound,
        ServerError,
        ValidationError,
        Unauthorized,
        Forbidden
    }

    public static class ResultErrorExtensions
    {
    }
}
