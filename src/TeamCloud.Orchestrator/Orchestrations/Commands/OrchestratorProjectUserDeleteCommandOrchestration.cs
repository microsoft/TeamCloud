/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Orchestrations.Commands.Activities;
using TeamCloud.Orchestrator.Orchestrations.Utilities;

namespace TeamCloud.Orchestrator.Orchestrations.Commands
{
    public static class OrchestratorProjectUserDeleteCommandOrchestration
    {
        [FunctionName(nameof(OrchestratorProjectUserDeleteCommandOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var orchestratorCommand = functionContext.GetInput<OrchestratorCommandMessage>();
            var command = orchestratorCommand.Command as OrchestratorProjectUserDeleteCommand;
            var commandResult = command.CreateResult();

            try
            {
                functionContext.SetCustomStatus("Waiting on for another project operation to complete.", log);

                await functionContext
                    .WaitForProjectCommandsAsync(command)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus("Waiting on providers to delete user.", log);

                // TODO: call set users on all providers (or project update for now)

                functionContext.SetCustomStatus($"Deleting user.", log);

                var user = await functionContext
                    .CallActivityWithRetryAsync<User>(nameof(ProjectUserDeleteActivity), (command.ProjectId.Value, command.Payload))
                    .ConfigureAwait(true);

                commandResult.Result = user;

                functionContext.SetCustomStatus($"User deleted.", log);
            }
            catch (Exception ex)
            {
                functionContext.SetCustomStatus("Failed to delete user.", log, ex);

                commandResult.Errors.Add(ex);

                throw;
            }
            finally
            {
                functionContext.SetOutput(commandResult);
            }
        }
    }
}
