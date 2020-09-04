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
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var command = functionContext.GetInput<ICommand>();
            var commandResult = command.CreateResult();
            var commandLog = functionContext.CreateReplaySafeLogger(log ?? NullLogger.Instance);

            try
            {
                functionContext.SetCustomStatus("Auditing command", log);

                await functionContext
                    .AuditAsync(command, commandResult)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus("Processing command", log);

                commandResult = await functionContext
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
                    functionContext.SetCustomStatus("Augmenting command result", log);

                    commandResult = await functionContext
                        .CallActivityWithRetryAsync<ICommandResult>(nameof(CommandResultAugmentActivity), commandResult)
                        .ConfigureAwait(true);
                }
                catch (Exception exc)
                {
                    commandResult ??= command.CreateResult();
                    commandResult.Errors.Add(exc);
                }

                functionContext.SetCustomStatus("Auditing command result", log);

                await functionContext
                    .AuditAsync(command, commandResult)
                    .ConfigureAwait(true);

                var commandException = commandResult.Errors?.ToException();

                if (commandException is null)
                    functionContext.SetCustomStatus($"Command succeeded", log);
                else
                    functionContext.SetCustomStatus($"Command failed: {commandException.Message}", log, commandException);

                functionContext.SetOutput(commandResult);
            }
        }
    }
}
