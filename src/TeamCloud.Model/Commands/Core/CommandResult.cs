/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Model.Commands.Serialization;

namespace TeamCloud.Model.Commands.Core
{
    [JsonConverter(typeof(CommandResultConverter))]
    public interface ICommandResult
    {
        Guid CommandId { get; set; }

        DateTime CreatedTime { get; set; }

        DateTime LastUpdatedTime { get; set; }

        CommandRuntimeStatus RuntimeStatus { get; set; }

        string CustomStatus { get; set; }

        IList<Exception> Errors { get; set; }

        [JsonProperty(Order = int.MaxValue, PropertyName = "_links")]
        Dictionary<string, string> Links { get; }
    }


    public interface ICommandResult<TResult> : ICommandResult
        where TResult : new()
    {
        TResult Result { get; set; }
    }


    public abstract class CommandResult<TResult> : ICommandResult<TResult>
        where TResult : new()
    {
        public Guid CommandId { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public CommandRuntimeStatus RuntimeStatus { get; set; }

        public string CustomStatus { get; set; }

        public TResult Result { get; set; }

        public IList<Exception> Errors { get; set; } = new List<Exception>();

        [JsonProperty(Order = int.MaxValue, PropertyName = "_links")]
        public Dictionary<string, string> Links { get; private set; } = new Dictionary<string, string>();
    }
}
