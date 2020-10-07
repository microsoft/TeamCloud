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
        protected OrchestratorCommand(CommandAction action, UserDocument user, TPayload payload) : base(action, user, payload)
        { }
    }

    public abstract class OrchestratorCommand<TPayload, TCommandResult, TProviderCommand, TProviderPayload> : OrchestratorCommand<TPayload, TCommandResult>
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
        where TProviderPayload : class, new()
        where TProviderCommand : IProviderCommand<TProviderPayload>
    {
        protected OrchestratorCommand(CommandAction action, UserDocument user, TPayload payload) : base(action, user, payload)
        { }
    }

    public abstract class OrchestratorCreateCommand<TPayload, TCommandResult> : OrchestratorCommand<TPayload, TCommandResult>
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
    {
        protected OrchestratorCreateCommand(UserDocument user, TPayload payload) : base(CommandAction.Create, user, payload)
        { }
    }

    public abstract class OrchestratorUpdateCommand<TPayload, TCommandResult> : OrchestratorCommand<TPayload, TCommandResult>
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
    {
        protected OrchestratorUpdateCommand(UserDocument user, TPayload payload) : base(CommandAction.Update, user, payload)
        { }
    }

    public abstract class OrchestratorDeleteCommand<TPayload, TCommandResult> : OrchestratorCommand<TPayload, TCommandResult>
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
    {
        protected OrchestratorDeleteCommand(UserDocument user, TPayload payload) : base(CommandAction.Delete, user, payload)
        { }
    }

    public abstract class OrchestratorCreateCommand<TPayload, TCommandResult, TProviderCommand, TProviderPayload> : OrchestratorCreateCommand<TPayload, TCommandResult>
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
        where TProviderPayload : class, new()
        where TProviderCommand : IProviderCommand<TProviderPayload>
    {
        protected OrchestratorCreateCommand(UserDocument user, TPayload payload) : base(user, payload)
        { }
    }

    public abstract class OrchestratorUpdateCommand<TPayload, TCommandResult, TProviderCommand, TProviderPayload> : OrchestratorUpdateCommand<TPayload, TCommandResult>
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
        where TProviderPayload : class, new()
        where TProviderCommand : IProviderCommand<TProviderPayload>
    {
        protected OrchestratorUpdateCommand(UserDocument user, TPayload payload) : base(user, payload)
        { }
    }

    public abstract class OrchestratorDeleteCommand<TPayload, TCommandResult, TProviderCommand, TProviderPayload> : OrchestratorDeleteCommand<TPayload, TCommandResult>
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
        where TProviderPayload : class, new()
        where TProviderCommand : IProviderCommand<TProviderPayload>
    {
        protected OrchestratorDeleteCommand(UserDocument user, TPayload payload) : base(user, payload)
        { }
    }

}
