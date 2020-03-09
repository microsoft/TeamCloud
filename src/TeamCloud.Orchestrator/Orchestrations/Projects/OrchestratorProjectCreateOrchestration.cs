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

namespace TeamCloud.Orchestrator.Orchestrations.Projects
{
    public static class OrchestratorProjectCreateOrchestration
    {
        [FunctionName(nameof(OrchestratorProjectCreateOrchestration))]
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

                project.TeamCloudId = commandMessage.TeamCloud.Id;
                project.Tags = commandMessage.TeamCloud.Tags.Merge(project.Tags);
                project.Properties = commandMessage.TeamCloud.Properties.Merge(project.Properties);

                functionContext.SetCustomStatus($"Creating project ...", log);

                project = await functionContext
                    .CallActivityWithRetryAsync<Project>(nameof(ProjectCreateActivity), project)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus($"Creating project resources ...", log);

                project = await CreateProjectResourcesAsync(functionContext, project)
                    .ConfigureAwait(true);

                var providerCommand = command is IOrchestratorCommandConvert commandConvert
                    ? commandConvert.CreateProviderCommand()
                    : throw new NotSupportedException($"Unable to convert command of type '{command.GetType()}' to '{typeof(IProviderCommand)}'");

                var providerResults = await functionContext
                    .SendCommandAsync<ICommandResult<ProviderOutput>>(providerCommand, project, commandMessage.TeamCloud)
                    .ConfigureAwait(true);

                project.Merge(providerResults);

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

            functionContext.StartNewOrchestration(nameof(OrchestratorProjectDeleteOrchestration), new OrchestratorCommandMessage(deleteCommand));
        }

        private static async Task<Project> CreateProjectResourcesAsync(IDurableOrchestrationContext functionContext, Project project)
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
