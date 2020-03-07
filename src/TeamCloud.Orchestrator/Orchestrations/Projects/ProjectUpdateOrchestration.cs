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
using TeamCloud.Orchestrator.Orchestrations.Projects.Activities;
using TeamCloud.Orchestrator.Orchestrations.Projects.Utilities;

namespace TeamCloud.Orchestrator.Orchestrations.Projects
{
    public static class ProjectUpdateOrchestration
    {
        [FunctionName(nameof(ProjectUpdateOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var orchestratorCommand = functionContext.GetInput<OrchestratorCommandMessage>();
            var command = orchestratorCommand.Command as ProjectUpdateCommand;
            var commandResult = command.CreateResult();

            try
            {
                functionContext.SetCustomStatus("Waiting on for another project operation to complete.", log);

                await functionContext
                    .WaitForProjectCommandsAsync(command)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus($"Updating project.", log);

                var user = command.User;
                var project = command.Payload;
                var teamCloud = orchestratorCommand.TeamCloud;

                project = await functionContext
                    .CallActivityWithRetryAsync<Project>(nameof(ProjectUpdateActivity), project)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus("Waiting on providers to update project resources.", log);

                await functionContext
                    .SendCommandAsync(command)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus("Project updated.", log);

                commandResult.Result = project;
            }
            catch (Exception ex)
            {
                functionContext.SetCustomStatus("Failed to update project.", log, ex);

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
