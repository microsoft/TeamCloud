/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.API.Data.Results
{
    public interface IErrorResult : IFailureResult
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
        public IList<ResultError> Errors { get; set; }


        public static ErrorResult BadRequest(List<ResultError> errors = null)
            => new ErrorResult { Code = StatusCodes.Status400BadRequest, Status = "BadRequest", Errors = errors };

        public static ErrorResult BadRequest(string message, ResultErrorCode code)
            => BadRequest(new List<ResultError> { new ResultError { Code = code, Message = message } });

        public static ErrorResult BadRequest(ValidationResult validation)
            => BadRequest(new List<ResultError> { ResultError.ValidationFailure(validation?.Errors ?? throw new ArgumentNullException(nameof(validation))) });

        public static ErrorResult BadRequest(ValidationError validationError)
            => BadRequest(new List<ResultError> { ResultError.ValidationFailure(validationError) });

        public static ErrorResult NotFound(string message)
            => new ErrorResult { Code = StatusCodes.Status404NotFound, Status = "NotFound", Errors = new List<ResultError> { ResultError.NotFound(message) } };

        public static ErrorResult Conflict(string message)
            => new ErrorResult { Code = StatusCodes.Status409Conflict, Status = "Conflict", Errors = new List<ResultError> { ResultError.Conflict(message) } };

        public static ErrorResult ServerError(IList<ResultError> errors = null)
            => new ErrorResult { Code = StatusCodes.Status500InternalServerError, Status = "ServerError", Errors = errors ?? new List<ResultError>() };

        public static ErrorResult ServerError(IList<Exception> exceptions)
            => new ErrorResult { Code = StatusCodes.Status500InternalServerError, Status = "ServerError", Errors = exceptions?.Select(e => ResultError.ServerError(e)).ToList() ?? new List<ResultError>() };

        public static ErrorResult Unauthorized()
            => new ErrorResult { Code = StatusCodes.Status401Unauthorized, Status = "Unauthorized", Errors = new List<ResultError> { ResultError.Unauthorized() } };

        public static ErrorResult Forbidden()
            => new ErrorResult { Code = StatusCodes.Status403Forbidden, Status = "Forbidden", Errors = new List<ResultError> { ResultError.Forbidden() } };

        public static ErrorResult Unknown(int? code)
            => new ErrorResult { Code = code ?? StatusCodes.Status500InternalServerError, Status = "Unknown", Errors = new List<ResultError> { ResultError.Unknown() } };
    }

    public static class ErrorResultExtensions
    {
        public static IActionResult ActionResult(this IErrorResult result) => result?.Code switch
        {
            StatusCodes.Status400BadRequest => new BadRequestObjectResult(result),
            StatusCodes.Status401Unauthorized => new JsonResult(result) { StatusCode = StatusCodes.Status401Unauthorized },
            StatusCodes.Status403Forbidden => new JsonResult(result) { StatusCode = StatusCodes.Status403Forbidden },
            StatusCodes.Status404NotFound => new NotFoundObjectResult(result),
            StatusCodes.Status409Conflict => new ConflictObjectResult(result),
            StatusCodes.Status500InternalServerError => new JsonResult(result) { StatusCode = StatusCodes.Status500InternalServerError },
            _ => throw new NotImplementedException()
        };
    }
}
