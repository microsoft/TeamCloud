/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities
{
    internal static class OrchestratorCommandExtensions
    {
        internal static async Task<ICommandResult> GetCommandResultAsync(this IDurableClient durableClient, ICommand command)
        {
            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            if (command is null)
                throw new ArgumentNullException(nameof(command));

            var commandStatus = await durableClient
                .GetStatusAsync(CommandTrigger.GetCommandOrchestrationInstanceId(command))
                .ConfigureAwait(false);

            if (commandStatus is null)
            {
                // the command orchestration wasn't created yet
                // there is no way to return a command result

                return null;
            }
            else if (commandStatus.RuntimeStatus.IsFinal())
            {
                // the command orchestration reached a final state
                // but the message orchestration is responsible to
                // send the result and there could modify the overall
                // command result (e.g. if a send operation fails).

                var commandMessageStatus = await durableClient
                    .GetStatusAsync(CommandTrigger.GetCommandWrapperOrchestrationInstanceId(command))
                    .ConfigureAwait(false);

                if (commandMessageStatus?.Output.HasValues ?? false)
                {
                    return commandMessageStatus.Output
                        .ToObject<ICommandResult>()
                        .ApplyStatus(commandMessageStatus);
                }
            }

            // the command orchestration is in-flight
            // get the current command result from its
            // output or fallback to the default result

            var commandResult = commandStatus.Output.HasValues
                ? commandStatus.Output.ToObject<ICommandResult>()
                : command.CreateResult(); // fallback to the default command result

            return commandResult.ApplyStatus(commandStatus);
        }

    }
}
