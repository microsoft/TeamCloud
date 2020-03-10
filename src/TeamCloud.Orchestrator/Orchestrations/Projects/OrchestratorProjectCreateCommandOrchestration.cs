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
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Orchestrations.Azure;
using TeamCloud.Orchestrator.Orchestrations.Projects.Activities;
using TeamCloud.Orchestrator.Orchestrations.Projects.Utilities;
using TeamCloud.Orchestrator.Orchestrations.Providers.Activities;
using TeamCloud.Orchestrator.Orchestrations.TeamCloud.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Projects
{
    public static class OrchestratorProjectCreateCommandOrchestration
    {
        [FunctionName(nameof(OrchestratorProjectCreateCommandOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var commandMessage = functionContext.GetInput<OrchestratorCommandMessage>();

            var command = (OrchestratorProjectCreateCommand)commandMessage.Command;
            var commandResult = command.CreateResult();

            var project = command.Payload;

            try
            {
                functionContext.SetCustomStatus("Waiting on for another project operation to complete.", log);

                await functionContext
                    .WaitForProjectCommandsAsync(command)
                    .ConfigureAwait(true);

                var teamCloud = await functionContext
                    .GetTeamCloudAsync()
                    .ConfigureAwait(true);

                project.TeamCloudId = teamCloud.Id;
                project.Tags = teamCloud.Tags.Merge(project.Tags);
                project.Properties = teamCloud.Properties.Merge(project.Properties);

                functionContext.SetCustomStatus($"Project {project.Id} - Creating ...", log);

                project = await functionContext
                    .CallActivityWithRetryAsync<Project>(nameof(ProjectCreateActivity), project)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus($"Project {project.Id} - Allocating subscription ...", log);

                var subscriptionId = await functionContext
                    .CallActivityWithRetryAsync<Guid>(nameof(AzureSubscriptionPoolSelectActivity), project)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus($"Project {project.Id} - Provisioning resource group ...", log);

                project.ResourceGroup = await functionContext
                    .CallActivityWithRetryAsync<AzureResourceGroup>(nameof(AzureResourceGroupCreateActivity), (project, subscriptionId))
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus($"Project {project.Id} - Updating resource group tags ...", log);

                await functionContext
                    .CallActivityWithRetryAsync(nameof(AzureResourceGroupTagActivity), project)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus($"Project {project.Id} - Updating ...", log);

                project = await functionContext
                    .CallActivityWithRetryAsync<Project>(nameof(ProjectUpdateActivity), project)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus($"Project {project.Id} - Preparing provider command ...", log);

                var providerCommand = command is IOrchestratorCommandConvert<Project> commandConvert
                    ? commandConvert.CreateProviderCommand(project)
                    : throw new NotSupportedException($"Unable to convert command of type '{command.GetType()}' to '{typeof(IProviderCommand)}'");

                functionContext.SetCustomStatus($"Project {project.Id} - Sending provider command ...", log);

                var providerResults = await functionContext
                    .SendCommandAsync<ICommandResult<ProviderOutput>>(providerCommand, project)
                    .ConfigureAwait(true);

                project.Merge(providerResults);

                project = await functionContext
                    .CallActivityWithRetryAsync<Project>(nameof(ProjectUpdateActivity), project)
                    .ConfigureAwait(true);

                commandResult.Result = project;
            }
            catch (Exception ex)
            {
                await RollbackProjectCreationAsync(functionContext, project)
                    .ConfigureAwait(false);

                functionContext.SetCustomStatus($"Failed to create project '{project.Id}'.", log, ex);

                commandResult.Errors.Add(ex);

                throw;
            }
            finally
            {
                functionContext.SetOutput(commandResult);
            }
        }

        private static async Task RollbackProjectCreationAsync(IDurableOrchestrationContext functionContext, Project project)
        {
            if (project is null)
                throw new ArgumentNullException(nameof(project));

            var systemUser = await functionContext
                .CallActivityWithRetryAsync<User>(nameof(SystemUserActivity), null)
                .ConfigureAwait(true);

            var deleteCommand = new OrchestratorProjectDeleteCommand(systemUser, project);

            functionContext.StartNewOrchestration(nameof(OrchestratorProjectDeleteCommandOrchestration), new OrchestratorCommandMessage(deleteCommand));
        }
    }
}
