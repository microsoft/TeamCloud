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
using TeamCloud.Orchestrator.Orchestrations.Projects.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Projects
{
    public static class ProjectUserUpdateOrchestration
    {
        [FunctionName(nameof(ProjectUserUpdateOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var orchestratorCommand = functionContext.GetInput<OrchestratorCommandMessage>();
            var command = orchestratorCommand.Command as ProjectUserDeleteCommand;
            var commandResult = command.CreateResult();

            try
            {
                functionContext.SetCustomStatus("Waiting on for another project operation to complete.", log);

                await functionContext
                    .WaitForProjectCommandsAsync(command)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus($"Updating user.", log);

                var user = await functionContext
                    .CallActivityAsync<User>(nameof(ProjectUserUpdateActivity), (command.ProjectId.Value, command.Payload))
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus("Waiting on providers to update user.", log);

                // TODO: call set users on all providers (or project update for now)

                commandResult.Result = user;

                functionContext.SetCustomStatus($"User updated.", log);
            }
            catch (Exception ex)
            {
                functionContext.SetCustomStatus("Failed to update user.", log, ex);

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
