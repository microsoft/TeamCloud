/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Handlers;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities
{
    internal static class OrchestratorCommandExtensions
    {
        internal static Task<ICommandResult> GetCommandResultAsync(this IDurableClient durableClient, ICommand command)
            => GetCommandResultAsync(durableClient, command?.CommandId ?? throw new ArgumentNullException(nameof(command)));

        internal static async Task<ICommandResult> GetCommandResultAsync(this IDurableClient durableClient, Guid commandId)
        {
            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            var commandStatus = await durableClient
                .GetStatusAsync(OrchestratorCommandOrchestrationHandler.GetCommandOrchestrationInstanceId(commandId))
                .ConfigureAwait(false);

            if (commandStatus?.RuntimeStatus.IsFinal() ?? true)
            {
                // we use the command wrapper status as a fallback if there is no orchstration status available
                // for the command itself, but also if the command orchestration reached a final state and we
                // need to return the overall processing status (incl. all tasks managed by the wrapper)

                commandStatus = await durableClient
                    .GetStatusAsync(OrchestratorCommandOrchestrationHandler.GetCommandOrchestrationWrapperInstanceId(commandId))
                    .ConfigureAwait(false) ?? commandStatus; // final fallback if there is no wrapper orchstration
            }

            if (commandStatus != null)
            {
                var commandResult = commandStatus.Output.HasValues
                    ? commandStatus.Output.ToObject<ICommandResult>()
                    : commandStatus.Input.ToObject<ICommand>().CreateResult(); // fallback to the default command result

                return commandResult.ApplyStatus(commandStatus);
            }

            return null;
        }

    }
}
