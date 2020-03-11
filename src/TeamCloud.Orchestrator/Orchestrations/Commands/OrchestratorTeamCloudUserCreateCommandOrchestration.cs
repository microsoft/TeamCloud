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
using TeamCloud.Orchestrator.Orchestrations.Commands.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Commands
{
    public static class OrchestratorTeamCloudUserCreateCommandOrchestration
    {
        [FunctionName(nameof(OrchestratorTeamCloudUserCreateCommandOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var orchestratorCommand = functionContext.GetInput<OrchestratorCommandMessage>();

            var command = (OrchestratorTeamCloudUserCreateCommand)orchestratorCommand.Command;
            var commandResult = command.CreateResult();

            var user = command.Payload;

            try
            {
                functionContext.SetCustomStatus($"Creating user.", log);

                var teamCloud = await functionContext
                    .GetTeamCloudAsync()
                    .ConfigureAwait(true);

                using (await functionContext.LockAsync(teamCloud).ConfigureAwait(true))
                {
                    teamCloud = await functionContext
                        .GetTeamCloudAsync()
                        .ConfigureAwait(true);

                    if (teamCloud.Users.Any(u => u.Id == user.Id))
                        throw new OrchestratorCommandException($"User '{user.Id}' already exists.", command);

                    teamCloud.Users.Add(user);

                    teamCloud = await functionContext
                        .SetTeamCloudAsync(teamCloud)
                        .ConfigureAwait(true);
                }

                var projects = await functionContext
                    .GetTeamCloudProjectsAsync()
                    .ConfigureAwait(true);

                foreach (var project in projects)
                {
                    var projectUpdateCommand = new OrchestratorProjectUpdateCommand(command.User, project);

                    functionContext.StartNewOrchestration(nameof(OrchestratorProjectUpdateCommand), new OrchestratorCommandMessage(projectUpdateCommand));
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
