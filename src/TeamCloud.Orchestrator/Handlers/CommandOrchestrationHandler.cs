/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Operations.Orchestrations.Utilities;

namespace TeamCloud.Orchestrator.Handlers
{
    public sealed class CommandOrchestrationHandler : ICommandHandler
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

        public bool CanHandle(ICommand command, bool fallback = false)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (fallback)
            {
                // this handler operates in fallback mode only !!!
                // so orchestration will be used as a fallback, if
                // there is no other handler that handles the
                // command in a first try.

                var orchestrationName = GetCommandOrchestrationName(command);

                return FunctionsEnvironment.FunctionExists(orchestrationName);
            }

            return false;
        }

        public async Task<ICommandResult> HandleAsync(ICommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient durableClient = null)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            var wrapperInstanceId = GetCommandOrchestrationWrapperInstanceId(command.CommandId);

            try
            {
                _ = await durableClient
                    .StartNewAsync(nameof(OrchestratorCommandOrchestration), wrapperInstanceId, command)
                    .ConfigureAwait(false);
            }
            catch (InvalidOperationException exc)
            {
                if ((await durableClient.GetCommandResultAsync(command).ConfigureAwait(false)) is null)
                {
                    throw; // bubble exception - as we can't find a command wrapper orchestration.
                }
                else
                {
                    throw new NotSupportedException($"Orchstration for command {command.CommandId} can only started once", exc);
                }
            }

            return await durableClient
                .GetCommandResultAsync(command)
                .ConfigureAwait(false);
        }
    }
}
