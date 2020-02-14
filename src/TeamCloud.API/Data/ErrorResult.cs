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

namespace TeamCloud.API.Data
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

        [JsonProperty("_trackingId", NullValueHandling = NullValueHandling.Ignore, Order = int.MaxValue)]
        public string CommandId { get; set; }


        public static ErrorResult BadRequest(List<ResultError> errors = null)
            => new ErrorResult { Code = StatusCodes.Status400BadRequest, Status = "BadRequest", Errors = errors };

        public static ErrorResult BadRequest(string message, ResultErrorCodes code)
            => BadRequest(new List<ResultError> { new ResultError { Code = code, Message = message } });

        public static ErrorResult BadRequest(ValidationResult validation)
            => BadRequest(new List<ResultError> { ResultError.ValidationFailure(validation.Errors) });

        public static ErrorResult BadRequest(ValidationError validationError)
            => BadRequest(new List<ResultError> { ResultError.ValidationFailure(validationError) });

        public static ErrorResult NotFound(string message)
            => new ErrorResult { Code = StatusCodes.Status404NotFound, Status = "NotFound", Errors = new List<ResultError> { ResultError.NotFound(message) } };

        public static ErrorResult Conflict(string message)
            => new ErrorResult { Code = StatusCodes.Status409Conflict, Status = "Conflict", Errors = new List<ResultError> { ResultError.Conflict(message) } };

        public static ErrorResult ServerError(IList<ResultError> errors = null, string commandId = null)
            => new ErrorResult { CommandId = commandId, Code = StatusCodes.Status500InternalServerError, Status = "ServerError", Errors = errors ?? new List<ResultError>() };

        public static ErrorResult ServerError(IList<Exception> exceptions, string commandId = null)
            => new ErrorResult { CommandId = commandId, Code = StatusCodes.Status500InternalServerError, Status = "ServerError", Errors = exceptions?.Select(e => ResultError.ServerError(e)).ToList() ?? new List<ResultError>() };
    }

    public static class ErrorResultExtensions
    {
        public static IActionResult ActionResult(this IErrorResult result) => result.Code switch
        {
            StatusCodes.Status400BadRequest => new BadRequestObjectResult(result),
            StatusCodes.Status404NotFound => new NotFoundObjectResult(result),
            StatusCodes.Status409Conflict => new ConflictObjectResult(result),
            StatusCodes.Status500InternalServerError => new JsonResult(result) { StatusCode = StatusCodes.Status500InternalServerError },
            _ => throw new NotImplementedException()
        };
    }
}
