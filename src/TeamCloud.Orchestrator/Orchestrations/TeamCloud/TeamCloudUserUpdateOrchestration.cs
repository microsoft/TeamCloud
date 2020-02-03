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
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Orchestrations.Projects.Activities;
using TeamCloud.Orchestrator.Orchestrations.TeamCloud.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.TeamCloud
{
    public static class TeamCloudUserUpdateOrchestration
    {
        [FunctionName(nameof(TeamCloudUserUpdateOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext
            /* ILogger log */)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var orchestratorCommand = functionContext.GetInput<OrchestratorCommandMessage>();

            var command = orchestratorCommand.Command as TeamCloudUserCreateCommand;

            var user = await functionContext
                .CallActivityAsync<User>(nameof(TeamCloudUserUpdateActivity), command.Payload)
                .ConfigureAwait(true);

            if (user.Role == UserRoles.TeamCloud.Admin)
            {
                var projects = await functionContext
                    .CallActivityAsync<List<Project>>(nameof(ProjectGetActivity), orchestratorCommand.TeamCloud)
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

            var commandResult = command.CreateResult();
            commandResult.Result = user;

            functionContext.SetOutput(commandResult);
        }
    }
}
