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
                functionContext.SetCustomStatus("Waiting on for another project operation to complete.", log);

                await functionContext
                    .WaitForProjectCommandsAsync(command)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus($"Creating project.", log);

                var teamCloud = commandMessage.TeamCloud;
                var project = command.Payload;

                project.TeamCloudId = teamCloud.Id;
                project.Tags = teamCloud.Tags.Merge(project.Tags);
                project.Properties = teamCloud.Properties.Merge(project.Properties);

                var providers = teamCloud.ProvidersFor(project);

                project.ProviderProperties = providers
                    .ToDictionary(provider => provider.Id, provider => provider.Properties);

                project = await functionContext
                    .CallActivityAsync<Project>(nameof(ProjectCreateActivity), project)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus("Creating Azure resource group.", log);

                var subscriptionId = await functionContext
                    .CallActivityAsync<Guid>(nameof(AzureSubscriptionPoolSelectActivity), project)
                    .ConfigureAwait(true);

                project.ResourceGroup = await functionContext
                    .CallActivityAsync<AzureResourceGroup>(nameof(AzureResourceGroupCreateActivity), (project, subscriptionId))
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus("Saving resource group details.", log);

                command.Payload = project = await functionContext
                    .CallActivityAsync<Project>(nameof(ProjectUpdateActivity), project)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus("Applying tags to resource group.", log);

                await functionContext
                    .CallActivityAsync(nameof(AzureResourceGroupTagActivity), project)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus("Waiting on providers to create project resources.", log);

                var providerCommandTasks = providers
                    .GetProviderCommandTasks(command, functionContext);

                var providerCommandResults = await Task
                    .WhenAll(providerCommandTasks)
                    .ConfigureAwait(true);

                var providerResults = providerCommandResults
                    .Cast<ProviderProjectCreateCommandResult>();

                functionContext.SetCustomStatus("Saving provider resource details.", log);

                foreach (var providerResult in providerResults)
                {
                    if (providerResult.Result?.Properties?.Any() ?? false)
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
                }

                project = await functionContext
                    .CallActivityAsync<Project>(nameof(ProjectUpdateActivity), project)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus("Project created.", log);

                commandResult.Result = project;
            }
            catch (FunctionFailedException functionException) when (functionException.InnerException is AzureDeploymentException deploymentException)
            {
                functionContext.SetCustomStatus("Failed to create resource group: {deploymentException.ResourceError}.", log, functionException);

                throw;
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
    }
}
