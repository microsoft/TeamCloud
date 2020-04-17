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
using TeamCloud.Model;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Orchestrations.Commands.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Commands
{
    public static class OrchestratorTeamCloudUserUpdateCommandOrchestration
    {
        [FunctionName(nameof(OrchestratorTeamCloudUserUpdateCommandOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var commandMessage = functionContext.GetInput<OrchestratorCommandMessage>();
            var command = (OrchestratorTeamCloudUserUpdateCommand)commandMessage.Command;
            var commandResult = command.CreateResult();
            var user = command.Payload;

            using (log.BeginCommandScope(command))
            {
                try
                {
                    functionContext.SetCustomStatus($"Updating user.", log);

                    using (await functionContext.LockAsync<TeamCloudInstance>(TeamCloudInstance.DefaultId).ConfigureAwait(true))
                    {
                        var teamCloud = await functionContext
                            .GetTeamCloudAsync()
                            .ConfigureAwait(true);

                        var userDelete = teamCloud.Users.SingleOrDefault(u => u.Id == user.Id);

                        if (userDelete is null)
                            throw new OrchestratorCommandException($"User '{user.Id}' does not exist.", command);
                        else
                            teamCloud.Users.Remove(userDelete);

                        teamCloud.Users.Add(user);

                        teamCloud = await functionContext
                            .SetTeamCloudAsync(teamCloud)
                            .ConfigureAwait(true);
                    }

                    var projects = await functionContext
                        .ListProjectsAsync()
                        .ConfigureAwait(true);

                    foreach (var project in projects)
                    {
                        var projectUpdateCommand = new OrchestratorProjectUpdateCommand(command.User, project);

                        functionContext.StartNewOrchestration(nameof(OrchestratorProjectUpdateCommand), new OrchestratorCommandMessage(projectUpdateCommand));
                    }
                    commandResult.Result = user;

                    functionContext.SetCustomStatus($"User updated.", log);
                }
                catch (Exception ex)
                {
                    functionContext.SetCustomStatus("Failed to update user.", log, ex);

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
}
