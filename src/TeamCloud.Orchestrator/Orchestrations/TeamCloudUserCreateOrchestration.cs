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
    public static class TeamCloudUserCreateOrchestration
    {
        [FunctionName(nameof(TeamCloudUserCreateOrchestration))]
        public static async Task<bool> RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext
            /* ILogger log */)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            (OrchestratorContext orchestratorContext, TeamCloudUserCreateCommand command) = functionContext.GetInput<(OrchestratorContext, TeamCloudUserCreateCommand)>();

            var user = command.Payload;

            var teamCloud = await functionContext
                .CallActivityAsync<TeamCloudInstance>(nameof(TeamCloudUserCreateActivity), (orchestratorContext.TeamCloud, user))
                .ConfigureAwait(true);

            if (user.Role == UserRoles.TeamCloud.Admin)
            {
                var projects = await functionContext
                    .CallActivityAsync<List<Project>>(nameof(ProjectGetActivity), teamCloud)
                    .ConfigureAwait(true);

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