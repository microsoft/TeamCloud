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

namespace TeamCloud
{
    public static class AddTeamCloudUserOrchestration
    {
        [FunctionName(nameof(AddTeamCloudUserOrchestration))]
        public static async Task<bool> RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            (OrchestratorContext orchestratorContext, TeamCloudUserDefinition userDefinition) input = functionContext.GetInput<(OrchestratorContext, TeamCloudUserDefinition)>();

            var userId = Guid.NewGuid().ToString();  // Call Microsoft Graph and Get User's ID using the email address

            var newUser = new TeamCloudUser {
                Id = userId,
                Role = input.userDefinition.Role,
                Tags = input.userDefinition.Tags
            };

            var teamCloud = await functionContext.CallActivityAsync<TeamCloud>(nameof(AddTeamCloudUserActivity), (input.orchestratorContext.TeamCloud, newUser));

            if (newUser.Role == TeamCloudUserRole.Admin)
            {
                var projects = await functionContext.CallActivityAsync<List<Project>>(nameof(GetProjectsActivity), teamCloud);

                // TODO: this should probably be done in parallel

                foreach (var project in projects)
                {
                    var projectContext = new ProjectContext(teamCloud, project, input.orchestratorContext.User.Id);

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