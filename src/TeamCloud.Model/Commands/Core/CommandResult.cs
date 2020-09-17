/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamCloud.Model.Commands.Core
{
    public abstract class CommandResult : ICommandResult
    {
        public static TimeSpan MaximumTimeout => TimeSpan.FromMinutes(30);

        public Guid CommandId { get; set; }

        public DateTime? CreatedTime { get; set; }

        public DateTime? LastUpdatedTime { get; set; }

        private CommandRuntimeStatus runtimeStatus = CommandRuntimeStatus.Unknown;

        public CommandRuntimeStatus RuntimeStatus
        {
            get => Errors?.Any(err => err.Severity == CommandErrorSeverity.Error) ?? false ? CommandRuntimeStatus.Failed : runtimeStatus;
            set => runtimeStatus = value;
        }

        public string CustomStatus { get; set; }

        public IList<CommandError> Errors { get; set; } = new List<CommandError>();

        public Dictionary<string, string> Links { get; private set; } = new Dictionary<string, string>();

        public object Result { get; set; }

        public TimeSpan Timeout { get; set; } = MaximumTimeout;
    }

    public abstract class CommandResult<TResult> : CommandResult, ICommandResult<TResult>
        where TResult : class, new()
    {
        public new TResult Result
        {
            get => base.Result as TResult;
            set => base.Result = value;
        }
    }
}
