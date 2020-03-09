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
        where TPayload : new()
        where TCommandResult : ICommandResult
    { }

    public interface IOrchestratorCommandConvert
    {
        IProviderCommand CreateProviderCommand();
    }

    public interface IOrchestratorCommandConvert<TPayload, TProviderCommand> : IOrchestratorCommandConvert
        where TPayload : new()
        where TProviderCommand : IProviderCommand<TPayload>
    {
        new TProviderCommand CreateProviderCommand();
    }

    public abstract class OrchestratorCommand<TPayload, TCommandResult> : Command<TPayload, TCommandResult>
        where TPayload : new()
        where TCommandResult : ICommandResult, new()
    {
        protected OrchestratorCommand(User user, TPayload payload) : base(user, payload)
        { }
    }

    public abstract class OrchestratorCommand<TPayload, TCommandResult, TProviderCommand> : OrchestratorCommand<TPayload, TCommandResult>, IOrchestratorCommandConvert<TPayload, TProviderCommand>
        where TPayload : new()
        where TCommandResult : ICommandResult, new()
        where TProviderCommand : IProviderCommand<TPayload>
    {
        protected OrchestratorCommand(User user, TPayload payload) : base(user, payload)
        { }

        public abstract TProviderCommand CreateProviderCommand();

        IProviderCommand IOrchestratorCommandConvert.CreateProviderCommand()
            => this.CreateProviderCommand();
    }
}
