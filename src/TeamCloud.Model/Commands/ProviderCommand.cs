/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public interface IProviderCommand : ICommand
    { }

    public interface IProviderCommand<TPayload> : ICommand<TPayload>, IProviderCommand
        where TPayload : new()
    { }

    public interface IProviderCommand<TPayload, TCommandResult> : ICommand<TPayload, TCommandResult>, IProviderCommand<TPayload>
        where TPayload : new()
        where TCommandResult : ICommandResult
    { }

    public abstract class ProviderCommand<TPayload, TCommandResult> : Command<TPayload, TCommandResult>, IProviderCommand<TPayload, TCommandResult>
        where TPayload : new()
        where TCommandResult : ICommandResult, new()
    {
        protected ProviderCommand(User user, TPayload payload, Guid? commandId = null)
            : base(user, payload)
        {
            if (commandId.HasValue) base.CommandId = commandId.Value;
        }
    }
}
