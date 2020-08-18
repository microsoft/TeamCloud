/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using System;

namespace TeamCloud.Model.Commands
{
    public interface IOrchestratorCommand : ICommand
    { }

    public interface IOrchestratorCommand<TPayload> : ICommand<UserDocument, TPayload>, IOrchestratorCommand
        where TPayload : new()
    { }

    public interface IOrchestratorCommand<TPayload, TCommandResult> : ICommand<UserDocument, TPayload, TCommandResult>, IOrchestratorCommand<TPayload>
        where TPayload : class, new()
        where TCommandResult : ICommandResult
    { }


    public abstract class OrchestratorCommand<TPayload, TCommandResult> : Command<UserDocument, TPayload, TCommandResult>, IOrchestratorCommand<TPayload, TCommandResult>
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
    {
        protected OrchestratorCommand(UserDocument user, TPayload payload) : base(user, payload)
        { }
    }

    public abstract class OrchestratorCommand<TPayload, TCommandResult, TProviderCommand, TProviderPayload> : OrchestratorCommand<TPayload, TCommandResult>
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
        where TProviderPayload : class, new()
        where TProviderCommand : IProviderCommand<TProviderPayload>
    {
        protected OrchestratorCommand(UserDocument user, TPayload payload) : base(user, payload)
        { }
    }
}
