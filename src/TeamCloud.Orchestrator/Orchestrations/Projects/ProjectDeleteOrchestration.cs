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
using TeamCloud.Orchestrator.Orchestrations.Azure;
using TeamCloud.Orchestrator.Orchestrations.Projects.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Projects
{
    public static class ProjectDeleteOrchestration
    {
        [FunctionName(nameof(ProjectDeleteOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var orchestratorCommand = functionContext.GetInput<OrchestratorCommandMessage>();
            var command = orchestratorCommand.Command as ProjectDeleteCommand;
            var commandResult = command.CreateResult();

            try
            {
                functionContext.SetCustomStatus("Waiting on for another project operation to complete.", log);

                await functionContext
                    .WaitForProjectCommandsAsync(command)
                    .ConfigureAwait(true);

                var user = command.User;
                var project = command.Payload;
                var teamCloud = orchestratorCommand.TeamCloud;

                functionContext.SetCustomStatus("Waiting on providers to delete project resources.", log);

                var providerCommandTasks = teamCloud.ProvidersFor(project).GetProviderCommandTasks(command, functionContext);
                var providerCommandResultMessages = await Task
                    .WhenAll(providerCommandTasks)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus("Deleting Azure resource group.", log);

                await functionContext
                    .CallActivityAsync<AzureResourceGroup>(nameof(AzureResourceGroupDeleteActivity), project.ResourceGroup)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus("Deleting project from database.", log);

                project = await functionContext
                    .CallActivityAsync<Project>(nameof(ProjectDeleteActivity), project)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus("Project deleted.", log);

                commandResult.Result = project;
            }
            catch (Exception ex)
            {
                functionContext.SetCustomStatus("Failed to delete project.", log, ex);

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
