/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Deployment;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Orchestrations.Azure;
using TeamCloud.Orchestrator.Orchestrations.Projects.Activities;
using TeamCloud.Orchestrator.Orchestrations.Providers;

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

            var orchestratorCommand = functionContext.GetInput<OrchestratorCommandMessage>();

            var command = orchestratorCommand.Command as ProjectCreateCommand;

            // Not sure if we need this here, create shouldn't ever wait on another command
            await functionContext
                .WaitForProjectCommandsAsync(command)
                .ConfigureAwait(true);

            functionContext.SetCustomStatus("Creating new Project");

            var user = command.User;
            var project = command.Payload;
            var teamCloud = orchestratorCommand.TeamCloud;
            var providers = teamCloud.ProvidersFor(project);

            project.TeamCloudId = teamCloud.Id;
            project.TeamCloudApplicationInsightsKey = teamCloud.ApplicationInsightsKey;

            project.Tags.Concat(teamCloud.Tags);
            project.Properties = teamCloud.Properties;

            project.ProviderProperties = providers
                .Select(p => (p.Id, p.Properties))
                .ToDictionary(t => t.Id, t => t.Properties);

            project = await functionContext
                .CallActivityAsync<Project>(nameof(ProjectCreateActivity), project)
                .ConfigureAwait(true);

            functionContext.SetCustomStatus("Creating new Resource Group for Project");

            var subscriptionId = await functionContext
                .CallActivityAsync<Guid>(nameof(AzureSubscriptionPoolSelectActivity), project)
                .ConfigureAwait(true);

            try
            {
                project.ResourceGroup = await functionContext
                    .CallActivityAsync<AzureResourceGroup>(nameof(AzureResourceGroupCreateActivity), (project, subscriptionId))
                    .ConfigureAwait(true);
            }
            catch (FunctionFailedException functionException) when (functionException.InnerException is AzureDeploymentException)
            {
                var deploymentExecption = functionException.InnerException as AzureDeploymentException;
                functionContext.CreateReplaySafeLogger(log).LogError(functionException, "Failed to create new Resource Group for Project\n{0}", deploymentExecption.ResourceError);
                functionContext.SetCustomStatus("Failed to create new Resource Group for Project");
                throw;
            }

            project = await functionContext
                .CallActivityAsync<Project>(nameof(ProjectUpdateActivity), project)
                .ConfigureAwait(true);

            functionContext.SetCustomStatus("Creating Project Resources");

            var providerCommandTasks = providers.GetProviderCommandTasks(command, functionContext);

            var providerCommandResults = await Task
                .WhenAll(providerCommandTasks)
                .ConfigureAwait(true);

            var providerResults = providerCommandResults
                .Cast<ProviderProjectCreateCommandResult>();

            foreach (var providerResult in providerResults)
            {
                foreach (var providerProperty in providerResult.Result.Properties)
                {
                    project.ProviderProperties[providerResult.ProviderId][providerProperty.Key] = providerProperty.Value;
                }
            }

            project = await functionContext
                .CallActivityAsync<Project>(nameof(ProjectUpdateActivity), project)
                .ConfigureAwait(true);

            functionContext.SetCustomStatus("Project Created");

            var commandResult = command.CreateResult();
            commandResult.Result = project;

            functionContext.SetOutput(commandResult);
        }
    }
}
