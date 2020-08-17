/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
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

        string ProjectId { get; }

        object User { get; set; }

        ICommandResult CreateResult();

        object Payload { get; set; }
    }

    public interface ICommand<TUser, TPayload> : ICommand
        where TUser : IUser, new()
        where TPayload : new()
    {
        new TUser User { get; set; }

        new TPayload Payload { get; set; }
    }

    public interface ICommand<TUser, TPayload, TCommandResult> : ICommand<TUser, TPayload>
        where TUser : class, IUser, new()
        where TPayload : class, new()
        where TCommandResult : ICommandResult
    {
        new TCommandResult CreateResult();
    }

}
