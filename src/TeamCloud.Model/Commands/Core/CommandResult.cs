/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
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

        object Result { get; set; }
    }


    public interface ICommandResult<TResult> : ICommandResult
        where TResult : new()
    {
        new TResult Result { get; set; }
    }

    public abstract class CommandResult : ICommandResult
    {
        public Guid CommandId { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        private CommandRuntimeStatus runtimeStatus = CommandRuntimeStatus.Unknown;

        public CommandRuntimeStatus RuntimeStatus
        {
            get => Errors?.Any() ?? false ? CommandRuntimeStatus.Failed : runtimeStatus;
            set => runtimeStatus = value;
        }

        public string CustomStatus { get; set; }

        public IList<Exception> Errors { get; set; } = new List<Exception>();

        public Dictionary<string, string> Links { get; private set; } = new Dictionary<string, string>();

        public object Result { get; set; }
    }

    public abstract class CommandResult<TResult> : CommandResult, ICommandResult<TResult>
        where TResult : new()
    {
        public new TResult Result { get; set; }
    }
}
