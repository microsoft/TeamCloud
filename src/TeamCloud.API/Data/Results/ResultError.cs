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
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.API.Data.Results
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ResultError
    {
        public ResultErrorCode Code { get; set; }

        public string Message { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ValidationError> Errors { get; set; }


        public static ResultError Failed(Exception exception)
        {
            if (exception is null)
                throw new ArgumentNullException(nameof(exception));

            return new ResultError
            {
                Code = ResultErrorCode.Failed,
                Message = $"Operation Failed: {exception.Message}"
            };
        }

        public static ResultError Failed(CommandError error)
        {
            if (error is null)
                throw new ArgumentNullException(nameof(error));

            return new ResultError
            {
                Code = ResultErrorCode.Failed,
                Message = $"Operation Failed: {error.Message}"
            };
        }

        public static ResultError Conflict(string message)
            => new ResultError
            {
                Code = ResultErrorCode.Conflict,
                Message = message
            };

        public static ResultError NotFound(string message)
            => new ResultError
            {
                Code = ResultErrorCode.NotFound,
                Message = message
            };

        public static ResultError ValidationFailure(IList<ValidationFailure> failures)
            => new ResultError
            {
                Code = ResultErrorCode.ValidationError,
                Message = "Validation Failed",
                Errors = failures.Select(f => new ValidationError { Field = f.PropertyName, Message = f.ErrorMessage }).ToList()
            };

        public static ResultError ValidationFailure(ValidationError validationError)
            => new ResultError
            {
                Code = ResultErrorCode.ValidationError,
                Message = "Validation Failed",
                Errors = new List<ValidationError> { validationError }
            };

        public static ResultError Unauthorized()
            => new ResultError
            {
                Code = ResultErrorCode.Unauthorized,
                Message = "Unauthorized"
            };

        public static ResultError Forbidden()
            => new ResultError
            {
                Code = ResultErrorCode.Forbidden,
                Message = "Forbidden"
            };

        public static ResultError ServerError(Exception exception)
        {
            if (exception is null)
                throw new ArgumentNullException(nameof(exception));

            return new ResultError
            {
                Code = ResultErrorCode.ServerError,
                Message = $"ServerError: {exception.Message}"
            };
        }

        public static ResultError Unknown()
            => new ResultError
            {
                Code = ResultErrorCode.Unknown,
                Message = "An unknown error occured."
            };
    }

    public class ValidationError
    {
        public string Field { get; set; }

        public string Message { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResultErrorCode
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
}
