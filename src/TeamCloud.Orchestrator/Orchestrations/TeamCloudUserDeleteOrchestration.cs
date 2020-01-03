/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model;
using TeamCloud.Orchestrator.Activities;

namespace TeamCloud.Orchestrator.Orchestrations
{
    public static class TeamCloudUserDeleteOrchestration
    {
        [FunctionName(nameof(TeamCloudUserDeleteOrchestration))]
        public static async Task<TeamCloudInstance> RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            (OrchestratorContext orchestratorContext, User deleteUser) = functionContext.GetInput<(OrchestratorContext, User)>();

            var teamCloud = await functionContext.CallActivityAsync<TeamCloudInstance>(nameof(TeamCloudUserDeleteActivity), (orchestratorContext.TeamCloud, deleteUser));

            // TODO: is this necessary?

            if (deleteUser.Role == UserRoles.TeamCloud.Admin)
            {
                var projects = await functionContext.CallActivityAsync<List<Project>>(nameof(ProjectGetActivity), teamCloud);

                // TODO: this should probably be done in parallel

                foreach (var project in projects)
                {
                    var projectContext = new ProjectContext(teamCloud, project, orchestratorContext.User.Id);

                    // TODO: call set users on all providers
                    // var tasks = input.teamCloud.Configuration.Providers.Select(p =>
                    //                 context.CallHttpAsync(HttpMethod.Post, p.Location, JsonConvert.SerializeObject(projectContext)));

                    // await Task.WhenAll(tasks);
                }
            }

            return teamCloud;
        }
    }
}