/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Activities;
using TeamCloud.Orchestrator.Entities;

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

            var command = functionContext.GetInput<OrchestratorTeamCloudUserUpdateCommand>();
            var commandResult = command.CreateResult();
            var user = command.Payload;

            using (log.BeginCommandScope(command))
            {
                try
                {
                    functionContext.SetCustomStatus($"Updating user.", log);

                    var existingUser = default(User);

                    using (await functionContext.LockAsync<User>(user.Id.ToString()).ConfigureAwait(true))
                    {
                        existingUser = await functionContext
                            .GetUserAsync(user.Id)
                            .ConfigureAwait(true);

                        if (existingUser is null)
                            throw new OrchestratorCommandException($"User '{user.Id}' does not exist.", command);

                        if (user.HasEqualTeamCloudInfo(existingUser))
                            throw new OrchestratorCommandException($"User '{user.Id}' TeamCloud details have not changed.", command);

                        user = await functionContext
                            .SetUserTeamCloudInfoAsync(user)
                            .ConfigureAwait(true);
                    }

                    var projects = default(IEnumerable<Project>);

                    // only update all projects if the updated user is an admin
                    // or the user was an admin before the update, otherwise
                    // only update member projects if users' properties changed
                    if (user.IsAdmin() || existingUser.IsAdmin())
                    {
                        projects = await functionContext
                            .ListProjectsAsync()
                            .ConfigureAwait(true);
                    }
                    else if (user.ProjectMemberships.Any() && !user.Properties.SequenceEqual(existingUser.Properties))
                    {
                        projects = await functionContext
                            .ListProjectsAsync(user.ProjectMemberships.Select(m => m.ProjectId).ToList())
                            .ConfigureAwait(true);
                    }

                    if (projects?.Any() ?? false)
                    {
                        foreach (var project in projects)
                        {
                            var projectUpdateCommand = new OrchestratorProjectUpdateCommand(command.User, project);

                            functionContext.StartNewOrchestration(nameof(OrchestratorProjectUpdateCommand), projectUpdateCommand);
                        }
                    }

                    commandResult.Result = user;

                    functionContext.SetCustomStatus($"User updated.", log);
                }
                catch (Exception ex)
                {
                    functionContext.SetCustomStatus("Failed to update user.", log, ex);

                    commandResult ??= command.CreateResult();
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
