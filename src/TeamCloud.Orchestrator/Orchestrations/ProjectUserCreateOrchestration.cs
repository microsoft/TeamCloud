/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
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
            (OrchestratorContext orchestratorContext, UserDefinition userDefinition) = functionContext.GetInput<(OrchestratorContext, UserDefinition)>();

            var userId = Guid.NewGuid();  // Call Microsoft Graph and Get User's ID using the email address

            var newUser = new User
            {
                Id = userId,
                Role = userDefinition.Role,
                Tags = userDefinition.Tags
            };

            var project = await functionContext.CallActivityAsync<Project>(nameof(ProjectUserCreateActivity), (orchestratorContext.Project, newUser));

            var projectContext = new ProjectContext(orchestratorContext.TeamCloud, project, orchestratorContext.User.Id);

            // TODO: call set users on all providers
            // var tasks = input.teamCloud.Configuration.Providers.Select(p =>
            //                 context.CallHttpAsync(HttpMethod.Post, p.Location, JsonConvert.SerializeObject(projectContext)));

            // await Task.WhenAll(tasks);

            return true;
        }
    }
}