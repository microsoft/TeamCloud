/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.API.Data
{
    public interface IStatusResult : ISuccessResult
    {
        string State { get; }

        // user-facing
        string StateMessage { get; }
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
        public string Location { get; private set; }

        [JsonProperty("_trackingId", NullValueHandling = NullValueHandling.Ignore, Order = int.MaxValue)]
        public string CommandId { get; set; }


        private StatusResult() { }

        public static StatusResult Ok(string commandId, string state = null, string stateMessage = null)
            => new StatusResult
            {
                CommandId = commandId,
                Code = StatusCodes.Status200OK,
                Status = "Ok",
                State = string.IsNullOrWhiteSpace(state) ? null : state,
                StateMessage = string.IsNullOrWhiteSpace(stateMessage) ? null : stateMessage
            };

        public static StatusResult Accepted(string commandId, string location, string state = null, string stateMessage = null)
            => new StatusResult
            {
                CommandId = commandId,
                Code = StatusCodes.Status202Accepted,
                Status = "Accepted",
                Location = location,
                State = string.IsNullOrWhiteSpace(state) ? null : state,
                StateMessage = string.IsNullOrWhiteSpace(stateMessage) ? null : stateMessage,
            };

        public static StatusResult Success(string commandId)
            => new StatusResult
            {
                CommandId = commandId,
                Code = StatusCodes.Status200OK,
                Status = "Ok",
                State = "Complete"
            };

        public static StatusResult Success(string commandId, string location)
            => new StatusResult
            {
                CommandId = commandId,
                Code = StatusCodes.Status302Found,
                Location = location,
                Status = "Found",
                State = "Complete"
            };
    }

    public static class StatusResultExtensions
    {
        public static IActionResult ActionResult(this IStatusResult result) => (result.Code) switch
        {
            StatusCodes.Status200OK => new OkObjectResult(result),
            StatusCodes.Status202Accepted => new AcceptedResult(result.Location, result),
            StatusCodes.Status302Found => new JsonResult(result) { StatusCode = StatusCodes.Status302Found },
            _ => throw new NotImplementedException()
        };
    }
}
