/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Activities;
using TeamCloud.Orchestrator.Orchestrations.Utilities;

namespace TeamCloud.Orchestrator.Orchestrations.Commands
{
    public static class OrchestratorProjectUpdateCommandOrchestration
    {
        [FunctionName(nameof(OrchestratorProjectUpdateCommandOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var command = functionContext.GetInput<OrchestratorProjectUpdateCommand>();
            var commandResult = command.CreateResult();

            using (log.BeginCommandScope(command))
            {

                var project = commandResult.Result = command.Payload;

                try
                {
                    functionContext.SetCustomStatus($"Updating project.", log);

                    project = await functionContext
                        .CallActivityWithRetryAsync<Project>(nameof(ProjectSetActivity), project)
                        .ConfigureAwait(true);

                    functionContext.SetCustomStatus("Waiting on providers to update project resources.", log);

                    var providerResults = await functionContext
                        .SendCommandAsync<ProviderProjectUpdateCommand, ProviderProjectUpdateCommandResult>(new ProviderProjectUpdateCommand(command.User, project, command.CommandId))
                        .ConfigureAwait(true);
                }
                catch (Exception exc)
                {
                    commandResult ??= command.CreateResult();
                    commandResult.Errors.Add(exc);

                    throw;
                }
                finally
                {
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
}
