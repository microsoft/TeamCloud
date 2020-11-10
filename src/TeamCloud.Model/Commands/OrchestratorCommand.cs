/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public interface IOrchestratorCommand : ICommand
    { }

    public interface IOrchestratorCommand<TPayload> : ICommand<TPayload>, IOrchestratorCommand
        where TPayload : new()
    { }

    public interface IOrchestratorCommand<TPayload, TCommandResult> : ICommand<TPayload, TCommandResult>, IOrchestratorCommand<TPayload>
        where TPayload : class, new()
        where TCommandResult : ICommandResult
    { }


    public abstract class OrchestratorCommand<TPayload, TCommandResult> : Command<TPayload, TCommandResult>, IOrchestratorCommand<TPayload, TCommandResult>
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
    {
        protected OrchestratorCommand(CommandAction action, User user, TPayload payload) : base(action, user, payload)
        { }
    }

    public abstract class OrchestratorCreateCommand<TPayload, TCommandResult> : OrchestratorCommand<TPayload, TCommandResult>
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
    {
        protected OrchestratorCreateCommand(User user, TPayload payload) : base(CommandAction.Create, user, payload)
        { }
    }

    public abstract class OrchestratorUpdateCommand<TPayload, TCommandResult> : OrchestratorCommand<TPayload, TCommandResult>
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
    {
        protected OrchestratorUpdateCommand(User user, TPayload payload) : base(CommandAction.Update, user, payload)
        { }
    }

    public abstract class OrchestratorDeleteCommand<TPayload, TCommandResult> : OrchestratorCommand<TPayload, TCommandResult>
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
    {
        protected OrchestratorDeleteCommand(User user, TPayload payload) : base(CommandAction.Delete, user, payload)
        { }
    }
}
