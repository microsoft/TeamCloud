/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using TeamCloud.Model.Commands.Serialization;

namespace TeamCloud.Model.Commands.Core
{
    [JsonConverter(typeof(CommandResultConverter))]
    public interface ICommandResult
    {
        Guid CommandId { get; set; }

        string OrganizationId { get; }

        CommandAction CommandAction { get; set; }

        DateTime? CreatedTime { get; set; }

        DateTime? LastUpdatedTime { get; set; }

        CommandRuntimeStatus RuntimeStatus { get; set; }

        string CustomStatus { get; set; }


        [SuppressMessage("Usage", "CA2227:Collection properties should be read only")]
        IList<CommandError> Errors { get; set; }

        [JsonProperty(Order = int.MaxValue, PropertyName = "_links")]
        Dictionary<string, string> Links { get; }

        object Result { get; set; }
    }

    public interface ICommandResult<TResult> : ICommandResult
        where TResult : class, new()
    {
        new TResult Result
        {
            get => (this as ICommandResult).Result as TResult;
            set => (this as ICommandResult).Result = value;
        }
    }
}
