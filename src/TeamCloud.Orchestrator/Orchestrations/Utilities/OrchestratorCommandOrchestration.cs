/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Activities;
using TeamCloud.Orchestrator.Handlers;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities
{
    public static class OrchestratorCommandOrchestration
    {
        [FunctionName(nameof(OrchestratorCommandOrchestration))]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,
            ILogger log)
        {
            if (orchestrationContext is null)
                throw new ArgumentNullException(nameof(orchestrationContext));

            var command = orchestrationContext.GetInput<ICommand>();
            var commandResult = command.CreateResult();
            var commandLog = orchestrationContext.CreateReplaySafeLogger(log ?? NullLogger.Instance);

            try
            {
                orchestrationContext.SetCustomStatus("Auditing command", log);

                await orchestrationContext
                    .AuditAsync(command, commandResult)
                    .ConfigureAwait(true);

                orchestrationContext.SetCustomStatus("Processing command", log);

                commandResult = await orchestrationContext
                    .CallSubOrchestratorWithRetryAsync<ICommandResult>(OrchestratorCommandOrchestrationHandler.GetCommandOrchestrationName(command), command.CommandId.ToString(), command)
                    .ConfigureAwait(true);
            }
            catch (Exception exc)
            {
                commandResult ??= command.CreateResult();
                commandResult.Errors.Add(exc);
            }
            finally
            {
                try
                {
                    orchestrationContext.SetCustomStatus("Augmenting command result", log);

                    commandResult = await orchestrationContext
                        .CallActivityWithRetryAsync<ICommandResult>(nameof(CommandResultAugmentActivity), commandResult)
                        .ConfigureAwait(true);
                }
                catch (Exception exc)
                {
                    commandResult ??= command.CreateResult();
                    commandResult.Errors.Add(exc);
                }

                orchestrationContext.SetCustomStatus("Auditing command result", log);

                await orchestrationContext
                    .AuditAsync(command, commandResult)
                    .ConfigureAwait(true);

                var commandException = commandResult.Errors?.ToException();

                if (commandException is null)
                    orchestrationContext.SetCustomStatus($"Command succeeded", log);
                else
                    orchestrationContext.SetCustomStatus($"Command failed: {commandException.Message}", log, commandException);

                orchestrationContext.SetOutput(commandResult);
            }
        }
    }

    internal static class OrchestratorCommandExtensions
    {
        internal static async Task<ICommand> GetCommandAsync(this IDurableClient durableClient, Guid commandId)
        {
            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            var commandStatus = await durableClient
                .GetStatusAsync(OrchestratorCommandOrchestrationHandler.GetCommandOrchestrationWrapperInstanceId(commandId))
                .ConfigureAwait(false);

            return (commandStatus?.Input?.HasValues ?? false)
                ? commandStatus.Input.ToObject<ICommand>()
                : null;
        }

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
