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
    public static class TeamCloudUserCreateOrchestration
    {
        [FunctionName(nameof(TeamCloudUserCreateOrchestration))]
        public static async Task<bool> RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            (OrchestratorContext orchestratorContext, TeamCloudUserCreateCommand command) = functionContext.GetInput<(OrchestratorContext, TeamCloudUserCreateCommand)>();

            var user = command.Payload;

            var teamCloud = await functionContext.CallActivityAsync<TeamCloudInstance>(nameof(TeamCloudUserCreateActivity), (orchestratorContext.TeamCloud, user));

            if (user.Role == UserRoles.TeamCloud.Admin)
            {
                var projects = await functionContext.CallActivityAsync<List<Project>>(nameof(ProjectGetActivity), teamCloud);

                // TODO: this should probably be done in parallel

                foreach (var project in projects)
                {
                    // TODO: call set users on all providers
                    // var tasks = input.teamCloud.Configuration.Providers.Select(p =>
                    //                 context.CallHttpAsync(HttpMethod.Post, p.Location, JsonConvert.SerializeObject(projectContext)));

                    // await Task.WhenAll(tasks);
                }
            }

            return true;
        }
    }
}