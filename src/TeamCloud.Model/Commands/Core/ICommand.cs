/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using Newtonsoft.Json;
using TeamCloud.Model.Commands.Serialization;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands.Core
{
    [JsonConverter(typeof(CommandConverter))]
    public interface ICommand : IValidatable
    {
        Guid CommandId { get; }

        Guid ParentId { get; set; }

        string OrganizationId { get; }

        CommandAction CommandAction { get; }

        string ProjectId { get; }

        User User { get; set; }

        ICommandResult CreateResult();

        object Payload { get; set; }
    }

    public interface ICommand<TPayload> : ICommand
        where TPayload : new()
    {
        new TPayload Payload { get; set; }
    }

    public interface ICommand<TPayload, TCommandResult> : ICommand<TPayload>
        where TPayload : class, new()
        where TCommandResult : ICommandResult
    {
        new TCommandResult CreateResult();
    }
}
