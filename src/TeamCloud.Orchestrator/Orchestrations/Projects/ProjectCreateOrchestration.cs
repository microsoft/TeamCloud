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
using TeamCloud.Orchestrator.Orchestrations.Azure;
using TeamCloud.Orchestrator.Orchestrations.Projects.Activities;
using TeamCloud.Orchestrator.Orchestrations.Projects.Utilities;

namespace TeamCloud.Orchestrator.Orchestrations.Projects
{
    public static class ProjectCreateOrchestration
    {
        [FunctionName(nameof(ProjectCreateOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var commandMessage = functionContext.GetInput<OrchestratorCommandMessage>();
            var command = commandMessage.Command as ProjectCreateCommand;
            var commandResult = command.CreateResult();

            try
            {
                functionContext.SetCustomStatus("Waiting on for another project operation to complete.", log);

                await functionContext
                    .WaitForProjectCommandsAsync(command)
                    .ConfigureAwait(true);

                var project = command.Payload;

                project.TeamCloudId = commandMessage.TeamCloud.Id;
                project.Tags = commandMessage.TeamCloud.Tags.Merge(project.Tags);
                project.Properties = commandMessage.TeamCloud.Properties.Merge(project.Properties);

                functionContext.SetCustomStatus($"Creating project ...", log);

                project = await functionContext
                    .CallActivityWithRetryAsync<Project>(nameof(ProjectCreateActivity), project)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus($"Creating project resources ...", log);

                project = await CreateProjectResources(functionContext, project)
                    .ConfigureAwait(true);

                var providerResults = await functionContext
                    .SendCommandAsync<ICommandResult<ProviderOutput>>(command, project, commandMessage.TeamCloud)
                    .ConfigureAwait(true);

                project.Merge(providerResults);

                commandResult.Result = project;
            }
            catch (Exception ex)
            {
                functionContext.SetCustomStatus("Failed to create project.", log, ex);

                commandResult.Errors.Add(ex);

                throw;
            }
            finally
            {
                functionContext.SetOutput(commandResult);
            }
        }

        private static async Task<Project> CreateProjectResources(IDurableOrchestrationContext functionContext, Project project)
        {
            if (project is null)
                throw new ArgumentNullException(nameof(project));

            var subscriptionId = await functionContext
                .CallActivityWithRetryAsync<Guid>(nameof(AzureSubscriptionPoolSelectActivity), project)
                .ConfigureAwait(true);

            project.ResourceGroup = await functionContext
                .CallActivityWithRetryAsync<AzureResourceGroup>(nameof(AzureResourceGroupCreateActivity), (project, subscriptionId))
                .ConfigureAwait(true);

            project = await functionContext
                .CallActivityWithRetryAsync<Project>(nameof(ProjectUpdateActivity), project)
                .ConfigureAwait(true);

            await functionContext
                .CallActivityWithRetryAsync(nameof(AzureResourceGroupTagActivity), project)
                .ConfigureAwait(true);

            return project;
        }
    }
}
