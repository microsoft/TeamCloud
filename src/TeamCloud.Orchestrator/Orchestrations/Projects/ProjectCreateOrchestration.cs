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
                await functionContext
                    .WaitForProjectCommandsAsync(command)
                    .ConfigureAwait(true);

                if (!functionContext.IsReplaying)
                    functionContext.SetCustomStatus("Creating new Project");

                var project = command.Payload;
                var teamCloud = commandMessage.TeamCloud;
                var providers = teamCloud.ProvidersFor(project);

                project.TeamCloudId = teamCloud.Id;
                project.TeamCloudApplicationInsightsKey = teamCloud.ApplicationInsightsKey; // TODO: why do we persist the AI IK on each project? and why on the TC instance?
                project.Tags = teamCloud.Tags.Merge(project.Tags);
                project.Properties = teamCloud.Properties.Merge(project.Properties);

                project.ProviderProperties = providers
                    .ToDictionary(provider => provider.Id, provider => provider.Properties);

                project = await functionContext
                    .CallActivityAsync<Project>(nameof(ProjectCreateActivity), project)
                    .ConfigureAwait(true);

                if (!functionContext.IsReplaying)
                    functionContext.SetCustomStatus("Creating new Resource Group for Project");

                var subscriptionId = await functionContext
                    .CallActivityAsync<Guid>(nameof(AzureSubscriptionPoolSelectActivity), project)
                    .ConfigureAwait(true);

                project.ResourceGroup = await functionContext
                    .CallActivityAsync<AzureResourceGroup>(nameof(AzureResourceGroupCreateActivity), (project, subscriptionId))
                    .ConfigureAwait(true);

                project = await functionContext
                    .CallActivityAsync<Project>(nameof(ProjectUpdateActivity), project)
                    .ConfigureAwait(true);

                await functionContext
                    .CallActivityAsync(nameof(AzureResourceGroupTagActivity), project)
                    .ConfigureAwait(true);

                if (!functionContext.IsReplaying)
                    functionContext.SetCustomStatus("Creating Project Resources");

                var providerCommandTasks = providers
                    .GetProviderCommandTasks(command, functionContext);

                var providerCommandResults = await Task
                    .WhenAll(providerCommandTasks)
                    .ConfigureAwait(true);

                var providerResults = providerCommandResults
                    .Cast<ProviderProjectCreateCommandResult>();

                foreach (var providerResult in providerResults)
                {
                    if (project.ProviderProperties.TryGetValue(providerResult.ProviderId, out var providerProperties))
                    {
                        project.ProviderProperties[providerResult.ProviderId] = providerProperties.Merge(providerResult.Result.Properties);
                    }
                    else
                    {
                        project.ProviderProperties.Add(providerResult.ProviderId, providerResult.Result.Properties);
                    }
                }

                project = await functionContext
                    .CallActivityAsync<Project>(nameof(ProjectUpdateActivity), project)
                    .ConfigureAwait(true);

                if (!functionContext.IsReplaying)
                    functionContext.SetCustomStatus("Project Created");

                commandResult.Result = project;
            }
            catch (FunctionFailedException functionException) when (functionException.InnerException is AzureDeploymentException deploymentException)
            {
                log.LogError(functionException, $"Failed to create new Resource Group for Project - {deploymentException.ResourceError}");

                functionContext.SetCustomStatus("Failed to create new Resource Group for Project");

                throw;
            }
            finally
            {
                functionContext.SetOutput(commandResult);
            }
        }
    }
}
