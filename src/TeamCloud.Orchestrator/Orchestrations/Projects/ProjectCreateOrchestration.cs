/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Deployments;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Context;
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
            ILogger logger)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            (OrchestratorContext orchestratorContext, ProjectCreateCommand command) = functionContext.GetInput<(OrchestratorContext, ProjectCreateCommand)>();

            // Not sure if we need this here, create shouldn't ever wait on another command
            await functionContext
                .WaitForProjectCommandsAsync(command)
                .ConfigureAwait(true);

            functionContext.SetCustomStatus("Creating new Project");

            var user = command.User;
            var project = command.Payload;
            var teamCloud = orchestratorContext.TeamCloud;
            project.TeamCloudId = teamCloud.Id;
            project.TeamCloudApplicationInsightsKey = teamCloud.ApplicationInsightsKey;
            //project.ProviderVariables = teamCloud.Configuration.Providers.Select(p => (p.Id, p.Variables)).ToDictionary(t => t.Id, t => t.Variables);

            project = await functionContext
                .CallActivityAsync<Project>(nameof(ProjectCreateActivity), project)
                .ConfigureAwait(true);

            // await CreateAzureResourceGroupAsync(functionContext, orchestratorContext, project, teamCloud).ConfigureAwait(true);

            functionContext.SetCustomStatus("Creating new Resource Group for Project");

            var subscriptionId = await functionContext
                .CallActivityAsync<Guid>(nameof(AzureSubscriptionPoolSelectActivity), teamCloud)
                .ConfigureAwait(true);



            try
            {
                project.ResourceGroup = await functionContext
                    .CallActivityAsync<AzureResourceGroup>(nameof(AzureResourceGroupCreateActivity), (orchestratorContext, project, subscriptionId))
                    .ConfigureAwait(true);
            }
            catch (FunctionFailedException functionException) when (functionException.InnerException is AzureDeploymentException)
            {
                var deploymentExecption = functionException.InnerException as AzureDeploymentException;
                functionContext.CreateReplaySafeLogger(logger).LogError(functionException, "Failed to create new Resource Group for Project\n{0}", deploymentExecption.ResourceError);
                functionContext.SetCustomStatus("Failed to create new Resource Group for Project");
                throw;
            }

            project = await functionContext
                .CallActivityAsync<Project>(nameof(ProjectUpdateActivity), project)
                .ConfigureAwait(true);

            functionContext.SetCustomStatus("Creating Project Resources");

            // Send create command to providers
            var projectContext = new ProjectContext(teamCloud, project, user);
            var providerCommands = teamCloud.Providers.Select(provider => new ProviderCommand { Command = command, Provider = provider });
            // Execute tasks
            var providerCommandTasks = providerCommands.Select(providerCommand => functionContext.CallSubOrchestratorAsync<ProviderCommandResult>(nameof(ProviderCommandOrchestration), providerCommand));
            var providerCommandResults = await Task.WhenAll(providerCommandTasks).ConfigureAwait(true);

            functionContext.SetCustomStatus("Project Created");
            //// Create and initialize providers...
            // await CreateProjectResourcesAsync(functionContext, teamCloud, projectContext*).ConfigureAwait(false);
            // await InitializeProjectResourcesAsync(functionContext, teamCloud, projectContext).ConfigureAwait(false);

            functionContext.SetOutput(project);
        }
    }
}
