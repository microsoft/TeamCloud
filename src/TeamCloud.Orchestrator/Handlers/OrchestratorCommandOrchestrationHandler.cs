/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Orchestrations.Utilities;

namespace TeamCloud.Orchestrator.Handlers
{
    public sealed class OrchestratorCommandOrchestrationHandler : IOrchestratorCommandHandler
    {
        internal static string GetCommandOrchestrationName(ICommand command)
            => $"{command.GetType().Name}Orchestration";

        internal static string GetCommandOrchestrationInstanceId(Guid commandId)
            => commandId.ToString();

        internal static string GetCommandOrchestrationInstanceId(ICommand command)
            => GetCommandOrchestrationInstanceId(command.CommandId);

        internal static string GetCommandOrchestrationWrapperInstanceId(Guid commandId)
            => $"{GetCommandOrchestrationInstanceId(commandId)}-wrapper";

        internal static string GetCommandOrchestrationWrapperInstanceId(ICommand command)
            => GetCommandOrchestrationWrapperInstanceId(command.CommandId);

        public bool CanHandle(IOrchestratorCommand orchestratorCommand)
        {
            if (orchestratorCommand is null)
                throw new ArgumentNullException(nameof(orchestratorCommand));

            var orchestrationName = GetCommandOrchestrationName(orchestratorCommand);

            return FunctionsEnvironment.FunctionExists(orchestrationName);
        }

        public async Task<ICommandResult> HandleAsync(IOrchestratorCommand orchestratorCommand, IDurableClient durableClient = null)
        {
            if (orchestratorCommand is null)
                throw new ArgumentNullException(nameof(orchestratorCommand));

            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            if (CanHandle(orchestratorCommand))
            {
                var wrapperInstanceId = GetCommandOrchestrationWrapperInstanceId(orchestratorCommand.CommandId);

                try
                {
                    _ = await durableClient
                        .StartNewAsync(nameof(OrchestratorCommandOrchestration), wrapperInstanceId, orchestratorCommand)
                        .ConfigureAwait(false);
                }
                catch (InvalidOperationException exc)
                {
                    if ((await durableClient.GetCommandResultAsync(orchestratorCommand).ConfigureAwait(false)) is null)
                    {
                        throw; // bubble exception - as we can't find a command wrapper orchestration.
                    }
                    else
                    {
                        throw new NotSupportedException($"Orchstration for command {orchestratorCommand.CommandId} can only started once", exc);
                    }
                }

                return await durableClient
                    .GetCommandResultAsync(orchestratorCommand)
                    .ConfigureAwait(false);
            }

            throw new NotImplementedException($"Missing orchestration to handle {orchestratorCommand.GetType().Name} at {GetType()}");
        }
    }
}
