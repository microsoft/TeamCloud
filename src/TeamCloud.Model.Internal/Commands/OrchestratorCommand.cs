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

    public interface IOrchestratorCommandConvert<TPayload> : IOrchestratorCommandConvert
        where TPayload : new()
    {
        new IProviderCommand<TPayload> CreateProviderCommand();

        IProviderCommand<TPayload> CreateProviderCommand(TPayload payloadOverride);
    }

    public interface IOrchestratorCommandConvert<TPayload, TProviderCommand> : IOrchestratorCommandConvert<TPayload>
        where TPayload : new()
        where TProviderCommand : IProviderCommand<TPayload>
    {
        new TProviderCommand CreateProviderCommand();

        new TProviderCommand CreateProviderCommand(TPayload payloadOverride);
    }

    public abstract class OrchestratorCommand<TPayload, TCommandResult> : Command<TPayload, TCommandResult>, IOrchestratorCommand<TPayload, TCommandResult>
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

        public TProviderCommand CreateProviderCommand(TPayload payloadOverride)
        {
            var providerCommand = this.CreateProviderCommand();

            providerCommand.Payload = payloadOverride;

            return providerCommand;
        }

        IProviderCommand IOrchestratorCommandConvert.CreateProviderCommand()
            => this.CreateProviderCommand();

        IProviderCommand<TPayload> IOrchestratorCommandConvert<TPayload>.CreateProviderCommand()
            => this.CreateProviderCommand();

        IProviderCommand<TPayload> IOrchestratorCommandConvert<TPayload>.CreateProviderCommand(TPayload payloadOverride)
            => this.CreateProviderCommand(payloadOverride);
    }
}
