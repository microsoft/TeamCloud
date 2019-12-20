/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TeamCloud.Model;
using TeamCloud.Orchestrator.Activities;

namespace TeamCloud.Orchestrator.Orchestrations
{
    public static class ProjectCreateOrchestration
    {
        [FunctionName(nameof(ProjectCreateOrchestration))]
        public static async Task<bool> RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            (OrchestratorContext orchestratorContext, ProjectDefinition projectDefinition) input = functionContext.GetInput<(OrchestratorContext, ProjectDefinition)>();

            var user = input.orchestratorContext.User;
            var teamCloud = input.orchestratorContext.TeamCloud;
            var projectDefinition = input.projectDefinition;

            functionContext.SetCustomStatus("Creating Project...");

            var projectId = Guid.NewGuid().ToString();

            var project = new Project {
                Id = projectId,
                Name = projectDefinition.Name,
                Identity = null,
                TeamCloudId = teamCloud.Id,
                TeamCloudApplicationInsightsKey = teamCloud.ApplicationInsightsKey,
                // add project creator as project owner
                Users = new List<ProjectUser> { new ProjectUser { Id = user.Id, Role = ProjectUserRole.Owner, Tags = user.Tags } },
                Tags = projectDefinition.Tags,
                ProviderVariables = teamCloud.Configuration.Providers.Select(p => (p.Id, p.Variables)).ToDictionary(t => t.Id, t => t.Variables)
            };

            // add project to db and add new project to teamcloud in db
            project = await functionContext.CallActivityAsync<Project>(nameof(ProjectCreateActivity), project);


            var resourceGroup = new AzureResourceGroup {
                SubscriptionId = "", // get sub ID
                ResourceGroupName = $"{teamCloud.Configuration.Azure.ResourceGroupNamePrefix}{projectDefinition.Name}", // validate/clean
                Region = teamCloud.Configuration.Azure.Region
            };

            // TODO: deploy new resoruce group for project

            resourceGroup.ResourceGroupId = ""; // get resource group id


            project.ResourceGroup = resourceGroup;

            project = await functionContext.CallActivityAsync<Project>(nameof(ProjectUpdateActivity), project);

            var projectContext = new ProjectContext(teamCloud, project, user.Id);

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

            return true;
        }
    }
}