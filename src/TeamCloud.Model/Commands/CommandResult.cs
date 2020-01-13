/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Model.Commands.Serialization;

namespace TeamCloud.Model.Commands
{
    [JsonConverter(typeof(CommandResultConverter))]
    public interface ICommandResult
    {
        Guid CommandId { get; }

        DateTime CreatedTime { get; set; }

        DateTime LastUpdatedTime { get; set; }

        CommandRuntimeStatus RuntimeStatus { get; set; }

        string CustomStatus { get; set; }

        [JsonProperty(Order = int.MaxValue, PropertyName = "_links")]
        Dictionary<string, string> Links { get; }
    }


    public class CommandResult : ICommandResult
    {
        public Guid CommandId { get; internal set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public CommandRuntimeStatus RuntimeStatus { get; set; }

        public string CustomStatus { get; set; }

        [JsonProperty(Order = int.MaxValue, PropertyName = "_links")]
        public Dictionary<string, string> Links { get; private set; } = new Dictionary<string, string>();

        public CommandResult(Guid commandId) => CommandId = commandId;
    }


    public interface ICommandResult<TResult> : ICommandResult
        where TResult : new()
    {
        TResult Result { get; set; }
    }


    public class CommandResult<TResult> : ICommandResult<TResult>
        where TResult : new()
    {
        public Guid CommandId { get; internal set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public CommandRuntimeStatus RuntimeStatus { get; set; }

        public string CustomStatus { get; set; }

        public TResult Result { get; set; }

        [JsonProperty(Order = int.MaxValue, PropertyName = "_links")]
        public Dictionary<string, string> Links { get; private set; } = new Dictionary<string, string>();

        public CommandResult(Guid commandId) => CommandId = commandId;
    }
}
