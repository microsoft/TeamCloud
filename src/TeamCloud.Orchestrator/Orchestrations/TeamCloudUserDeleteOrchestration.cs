/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Context;
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Activities;

namespace TeamCloud.Orchestrator.Orchestrations
{
    public static class TeamCloudUserDeleteOrchestration
    {
        [FunctionName(nameof(TeamCloudUserDeleteOrchestration))]
        public static async Task<TeamCloudInstance> RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext
            /* ILogger log */)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            (OrchestratorContext orchestratorContext, TeamCloudUserDeletCommand command) = functionContext.GetInput<(OrchestratorContext, TeamCloudUserDeletCommand)>();

            var teamCloud = await functionContext
                .CallActivityAsync<TeamCloudInstance>(nameof(TeamCloudUserDeleteActivity), (orchestratorContext.TeamCloud, command.Payload))
                .ConfigureAwait(true);

            // TODO: is this necessary?

            if (command.Payload.Role == UserRoles.TeamCloud.Admin)
            {
                var projects = await functionContext
                    .CallActivityAsync<List<Project>>(nameof(ProjectGetActivity), teamCloud)
                    .ConfigureAwait(true); ;

                // TODO: this should probably be done in parallel

                foreach (var project in projects)
                {
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