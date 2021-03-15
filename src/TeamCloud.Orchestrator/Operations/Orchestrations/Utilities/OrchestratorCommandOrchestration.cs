/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading.Tasks;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Handlers;
using TeamCloud.Orchestrator.Operations.Activities;

namespace TeamCloud.Orchestrator.Operations.Orchestrations.Utilities
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
                    .CallSubOrchestratorWithRetryAsync<ICommandResult>(CommandOrchestrationHandler.GetCommandOrchestrationName(command), command.CommandId.ToString(), command)
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
                        .CallActivityWithRetryAsync<ICommandResult>(nameof(CommandResultActivity), new CommandResultActivity.Input() { CommandResult = commandResult })
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
}
