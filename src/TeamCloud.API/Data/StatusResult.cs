/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.API.Data
{
    public interface IStatusResult : IReturnResult
    {
        string State { get; }

        // user-facing
        string StateMessage { get; }

        string Location { get; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class StatusResult : IStatusResult
    {
        [JsonProperty(Order = int.MinValue)]
        public int Code { get; private set; }

        [JsonProperty(Order = int.MinValue)]
        public string Status { get; private set; }

        public string State { get; private set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string StateMessage { get; private set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ResultError> Errors { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Location { get; private set; }

        private StatusResult() { }

        public static StatusResult Ok(string state = null, string stateMessage = null)
        {
            var result = new StatusResult();
            result.Code = 200;
            result.Status = "Ok";
            result.State = state;
            result.StateMessage = stateMessage;
            return result;
        }

        public static StatusResult Accepted(string location, string state = null, string stateMessage = null)
        {
            var result = new StatusResult();
            result.Code = 202;
            result.Status = "Accepted";
            result.Location = location;
            result.State = state;
            result.StateMessage = stateMessage;
            return result;
        }

        public static StatusResult Success(string location)
        {
            var result = new StatusResult();
            result.Code = 302;
            result.Location = location;
            result.Status = "Found";
            result.State = "Complete";
            return result;
        }

        public static StatusResult Failed(List<ResultError> errors = null)
        {
            var result = new StatusResult();
            result.Code = 302;
            result.Status = "Found";
            result.State = "Failed";
            result.Errors = errors;
            return result;
        }

        public static StatusResult NotFound()
        {
            var result = new StatusResult();
            result.Code = 404;
            result.Status = "NotFound";
            return result;
        }
    }

    public static class StatusResultExtensions
    {
        public static IActionResult ActionResult(this IStatusResult result) => (result.Code) switch
        {
            200 => new OkObjectResult(result),
            202 => new AcceptedResult(result.Location, result),
            302 => new JsonResult(result) { StatusCode = 302 },
            404 => new NotFoundObjectResult(result),
            _ => throw new NotImplementedException()
        };
    }
}
