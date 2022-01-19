/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.API.Data.Results;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public sealed class StatusResult : IStatusResult
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

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public IList<ResultError> Errors { get; set; } = null;

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

    public static StatusResult Success(string commandId, string state = null, string stateMessage = null)
        => new StatusResult
        {
            CommandId = commandId,
            Code = StatusCodes.Status200OK,
            Status = "Ok",
            State = string.IsNullOrWhiteSpace(state) ? null : state,
            StateMessage = string.IsNullOrWhiteSpace(stateMessage) ? null : stateMessage,
        };

    public static StatusResult Success(string commandId, string location, string state = null, string stateMessage = null)
        => new StatusResult
        {
            CommandId = commandId,
            Location = location,
            Code = StatusCodes.Status302Found,
            Status = "Found",
            State = string.IsNullOrWhiteSpace(state) ? null : state,
            StateMessage = string.IsNullOrWhiteSpace(stateMessage) ? null : stateMessage,
        };

    public static StatusResult Failed(IEnumerable<ResultError> errors = null, string commandId = null, string state = null, string stateMessage = null)
        => new StatusResult
        {
            CommandId = commandId,
            Code = StatusCodes.Status200OK,
            Status = "Failed",
            State = string.IsNullOrWhiteSpace(state) ? null : state,
            StateMessage = string.IsNullOrWhiteSpace(stateMessage) ? null : stateMessage,
            Errors = errors?.ToList() ?? new List<ResultError>()
        };

    public static StatusResult Failed(IEnumerable<CommandError> errors, string commandId = null, string state = null, string stateMessage = null)
        => new StatusResult
        {
            CommandId = commandId,
            Code = StatusCodes.Status200OK,
            Status = "Failed",
            State = string.IsNullOrWhiteSpace(state) ? null : state,
            StateMessage = string.IsNullOrWhiteSpace(stateMessage) ? null : stateMessage,
            Errors = errors?.Select(error => ResultError.Failed(error)).ToList() ?? new List<ResultError>()
        };
}
