/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
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
    public static class ProjectUserCreateOrchestration
    {
        [FunctionName(nameof(ProjectUserCreateOrchestration))]
        public static async Task<bool> RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            (OrchestratorContext orchestratorContext, ProjectUserDefinition userDefinition) input = functionContext.GetInput<(OrchestratorContext, ProjectUserDefinition)>();

            var userId = Guid.NewGuid().ToString();  // Call Microsoft Graph and Get User's ID using the email address

            var newUser = new ProjectUser {
                Id = userId,
                Role = input.userDefinition.Role,
                Tags = input.userDefinition.Tags
            };

            var project = await functionContext.CallActivityAsync<Project>(nameof(ProjectUserCreateActivity), (input.orchestratorContext.Project, newUser));

            var projectContext = new ProjectContext(input.orchestratorContext.TeamCloud, project, input.orchestratorContext.User.Id);

            // TODO: call set users on all providers
            // var tasks = input.teamCloud.Configuration.Providers.Select(p =>
            //                 context.CallHttpAsync(HttpMethod.Post, p.Location, JsonConvert.SerializeObject(projectContext)));

            // await Task.WhenAll(tasks);

            return true;
        }
    }
}