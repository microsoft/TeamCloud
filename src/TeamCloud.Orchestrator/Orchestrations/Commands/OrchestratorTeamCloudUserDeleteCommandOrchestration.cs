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
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data.Core;
using TeamCloud.Model.Internal;
using TeamCloud.Model.Internal.Data;
using TeamCloud.Model.Internal.Commands;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Activities;
using TeamCloud.Orchestrator.Entities;

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

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var command = functionContext.GetInput<OrchestratorTeamCloudUserDeleteCommand>();
            var commandResult = command.CreateResult();
            var user = command.Payload;

            using (log.BeginCommandScope(command))
            {
                try
                {
                    functionContext.SetCustomStatus($"Deleting user.", log);

                    using (await functionContext.LockAsync<User>(user.Id.ToString()).ConfigureAwait(true))
                    {
                        await functionContext
                            .DeleteUserAsync(user.Id)
                            .ConfigureAwait(true);
                    }

                    var projects = default(IEnumerable<Project>);

                    // TODO: this is totally wrong
                    // only update all projects if user was an admin
                    if (user.IsAdmin())
                    {
                        projects = await functionContext
                            .ListProjectsAsync()
                            .ConfigureAwait(true);
                    }
                    else if (user.ProjectMemberships.Any())
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

                            functionContext.StartNewOrchestration(nameof(OrchestratorProjectUpdateCommandOrchestration), projectUpdateCommand);
                        }
                    }

                    commandResult.Result = user;

                    functionContext.SetCustomStatus($"User deleted.", log);
                }
                catch (Exception ex)
                {
                    functionContext.SetCustomStatus("Failed to delete user.", log, ex);

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
