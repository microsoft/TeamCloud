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

            // add project to db and add new project to teamcloud in db
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
                SubscriptionId = "", // get sub ID
                ResourceGroupName = $"{teamCloud.Configuration.Azure.ResourceGroupNamePrefix}{project.Name}", // validate/clean
                Region = teamCloud.Configuration.Azure.Region
            };

            // TODO: deploy new resoruce group for project

            resourceGroup.Id = Guid.NewGuid(); // TODO: get resource group id


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