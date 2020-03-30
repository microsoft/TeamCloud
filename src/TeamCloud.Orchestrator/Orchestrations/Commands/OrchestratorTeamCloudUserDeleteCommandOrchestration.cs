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
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Orchestrations.Commands.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Commands
{
    public static class OrchestratorTeamCloudUserDeleteCommandOrchestration
    {
        [FunctionName(nameof(OrchestratorTeamCloudUserDeleteCommandOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var orchestratorCommand = functionContext.GetInput<OrchestratorCommandMessage>();

            var command = (OrchestratorTeamCloudUserDeleteCommand)orchestratorCommand.Command;
            var commandResult = command.CreateResult();

            var user = command.Payload;

            try
            {
                functionContext.SetCustomStatus($"Deleting user.", log);

                using (await functionContext.LockAsync<TeamCloudInstance>(TeamCloudInstance.DefaultId).ConfigureAwait(true))
                {
                    var teamCloud = await functionContext
                        .GetTeamCloudAsync()
                        .ConfigureAwait(true);

                    var userDelete = teamCloud.Users.SingleOrDefault(u => u.Id == user.Id);

                    if (userDelete is null)
                        throw new OrchestratorCommandException($"User '{user.Id}' does not exist.", command);

                    teamCloud.Users.Remove(userDelete);

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

                functionContext.SetCustomStatus($"User deleted.", log);
            }
            catch (Exception ex)
            {
                functionContext.SetCustomStatus("Failed to delete user.", log, ex);

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
