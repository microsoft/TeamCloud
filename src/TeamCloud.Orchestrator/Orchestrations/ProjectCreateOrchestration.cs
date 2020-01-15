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
using TeamCloud.Model.Commands;
using TeamCloud.Model.Context;
using TeamCloud.Model.Data;
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
            project = await functionContext.CallActivityAsync<Project>(nameof(ProjectCreateActivity), project).ConfigureAwait(false);

            //// TODO: Create identity (service principal) for Project
            //var projectIdentity = new AzureIdentity
            //{
            //    Id = Guid.NewGuid(),
            //    AppId = "",
            //    Secret = ""
            //};
            //// Save the updated project back into the database
            //project = await functionContext.CallActivityAsync<Project>(nameof(ProjectUpdateActivity), project).ConfigureAwait(false);

            // Determine an Azure subscription from the SubscriptionPool property of TeamCloudAzureConfiguration
            var subscriptionID = await functionContext.CallActivityAsync<Guid>(nameof(AzureSubscriptionPoolSelectActivity), teamCloud).ConfigureAwait(false);

            // Create a new Azure resource group for the project
            project = await functionContext.CallActivityAsync<Project>(nameof(AzureResourceGroupCreateActivity), (teamCloud, project, subscriptionID)).ConfigureAwait(false);
            
            // Save the updated project back into the database
            project = await functionContext.CallActivityAsync<Project>(nameof(ProjectUpdateActivity), project).ConfigureAwait(false);

            var projectContext = new ProjectContext(teamCloud, project, command.User);
            functionContext.SetCustomStatus("Creating Project Resources...");

            //var p1 = teamCloud.Configuration.Providers[0];
            //var p2 = teamCloud.Configuration.Providers[1];
            //var c1 = p1.Dependencies.Init;
            //project.ProviderVariables[p2.Id] = null; // TODO call create on P2
            //project.ProviderVariables[c]

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