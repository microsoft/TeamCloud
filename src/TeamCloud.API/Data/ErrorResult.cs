/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.API.Data
{
    public interface IErrorResult : IReturnResult
    {
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ErrorResult : IErrorResult
    {
        [JsonProperty(Order = int.MinValue)]
        public int Code { get; private set; }

        [JsonProperty(Order = int.MinValue)]
        public string Status { get; private set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ResultError> Errors { get; set; }

        private ErrorResult() { }

        public static ErrorResult Ok()
            => new ErrorResult() { Code = 200, Status = "Ok" };

        public static ErrorResult BadRequest(List<ResultError> errors = null)
            => new ErrorResult() { Code = 400, Status = "BadRequest", Errors = errors };

        public static ErrorResult BadRequest(string message, ResultErrorCodes code)
            => ErrorResult.BadRequest(new List<ResultError> { new ResultError { Code = code, Message = message } });

        public static ErrorResult BadRequest(ValidationResult validation)
            => ErrorResult.BadRequest(validation.Errors.Select(failure => ResultError.ValidationFailure(failure)).ToList());

        public static ErrorResult NotFound(string message)
            => new ErrorResult { Code = 404, Status = "NotFound", Errors = new List<ResultError> { ResultError.NotFound(message) } };

        public static ErrorResult Conflict(string message)
            => new ErrorResult { Code = 409, Status = "Conflict", Errors = new List<ResultError> { ResultError.Conflict(message) } };
    }

    public static class ErrorResultExtensions
    {
        public static IActionResult ActionResult(this IErrorResult result) => (result.Code) switch
        {
            400 => new BadRequestObjectResult(result),
            404 => new NotFoundObjectResult(result),
            409 => new ConflictObjectResult(result),
            _ => throw new NotImplementedException()
        };
    }
}
