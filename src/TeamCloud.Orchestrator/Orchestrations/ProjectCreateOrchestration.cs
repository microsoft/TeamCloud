/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TeamCloud.Model;
using TeamCloud.Orchestrator.Activities;

namespace TeamCloud.Orchestrator.Orchestrations
{
    public static class ProjectCreateOrchestration
    {
        [FunctionName(nameof(ProjectCreateOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            (OrchestratorContext orchestratorContext, ProjectCreateCommand command) = functionContext.GetInput<(OrchestratorContext, ProjectCreateCommand)>();

            var user = command.User;
            var project = command.Payload;
            var teamCloud = orchestratorContext.TeamCloud;

            functionContext.SetCustomStatus("Creating Project...");

            project.TeamCloudId = teamCloud.Id;
            project.TeamCloudApplicationInsightsKey = teamCloud.ApplicationInsightsKey;
            project.ProviderVariables = teamCloud.Configuration.Providers.Select(p => (p.Id, p.Variables)).ToDictionary(t => t.Id, t => t.Variables);

            // Add project to db and add new project to teamcloud in db
            project = await functionContext.CallActivityAsync<Project>(nameof(ProjectCreateActivity), project);

            // TODO: Create identity (service principal) for Project

            var projectIdentity = new AzureIdentity
            {
                Id = Guid.NewGuid(),
                AppId = "",
                Secret = ""
            };

            project = await functionContext.CallActivityAsync<Project>(nameof(ProjectUpdateActivity), project);

            var resourceGroup = new AzureResourceGroup
            {
                SubscriptionId = teamCloud.Configuration.Azure.SubscriptionId,
                ResourceGroupName = $"{teamCloud.Configuration.Azure.ResourceGroupNamePrefix}{project.Name}", // TODO validate/clean
                Region = teamCloud.Configuration.Azure.Region
            };

            // Create new resource group for project
            resourceGroup.Id = await functionContext.CallActivityAsync<Guid>(nameof(AzureResourceGroupCreateActivity), project);

            // Assign resource group to project
            project.ResourceGroup = resourceGroup;

            project = await functionContext.CallActivityAsync<Project>(nameof(ProjectUpdateActivity), project);

            var projectContext = new ProjectContext(teamCloud, project, command.User);

            functionContext.SetCustomStatus("Creating Project Resources...");

            // TODO: call create on all providers (handeling dependencies)
            // var tasks = teamCloud.Configuration.Providers.Select(p =>
            //                 functionContext.CallHttpAsync(HttpMethod.Post, p.Location, JsonConvert.SerializeObject(projectContext)));

            // await Task.WhenAll(tasks);

            functionContext.SetCustomStatus("Initializing Project Resources...");

            // TODO: call init on all providers (handeling dependencies)
            // var tasks = teamCloud.Configuration.Providers.Select(p =>
            //                 functionContext.CallHttpAsync(HttpMethod.Post, p.Location, JsonConvert.SerializeObject(projectContext)));

            // await Task.WhenAll(tasks);

            functionContext.SetOutput(project);

            //return true;
        }
    }
}