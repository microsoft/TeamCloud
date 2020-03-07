/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Orchestrations.Projects.Activities;
using TeamCloud.Orchestrator.Orchestrations.TeamCloud.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.TeamCloud
{
    public static class TeamCloudUserCreateOrchestration
    {
        [FunctionName(nameof(TeamCloudUserCreateOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var orchestratorCommand = functionContext.GetInput<OrchestratorCommandMessage>();
            var command = orchestratorCommand.Command as TeamCloudUserCreateCommand;
            var commandResult = command.CreateResult();

            try
            {
                functionContext.SetCustomStatus($"Creating user.", log);

                var user = await functionContext
                    .CallActivityWithRetryAsync<User>(nameof(TeamCloudUserCreateActivity), command.Payload)
                    .ConfigureAwait(true);

                if (user.Role == UserRoles.TeamCloud.Admin)
                {
                    functionContext.SetCustomStatus("Waiting on project providers to create user.", log);

                    var projects = await functionContext
                        .CallActivityWithRetryAsync<List<Project>>(nameof(ProjectListActivity), orchestratorCommand.TeamCloud)
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

                commandResult.Result = user;

                functionContext.SetCustomStatus($"User created.", log);
            }
            catch (Exception ex)
            {
                functionContext.SetCustomStatus("Failed to create user.", log, ex);

                commandResult.Errors.Add(ex);

                throw;
            }
            finally
            {
                functionContext.SetOutput(commandResult);
            }
        }
    }
}
