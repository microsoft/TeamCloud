/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Internal;
using TeamCloud.Model.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Activities;
using TeamCloud.Orchestrator.Entities;
using TeamCloud.Model.Data;

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

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var command = functionContext.GetInput<OrchestratorTeamCloudUserCreateCommand>();
            var commandResult = command.CreateResult();
            var user = command.Payload;

            using (log.BeginCommandScope(command))
            {
                try
                {
                    functionContext.SetCustomStatus($"Creating user.", log);

                    using (await functionContext.LockContainerDocumentAsync(user).ConfigureAwait(true))
                    {
                        var existingUser = await functionContext
                            .GetUserAsync(user.Id)
                            .ConfigureAwait(true);

                        if (existingUser != null)
                            throw new OrchestratorCommandException($"User '{user.Id}' already exist.", command);

                        user = await functionContext
                            .SetUserTeamCloudInfoAsync(user)
                            .ConfigureAwait(true);
                    }

                    // this will only be called on a newly created user with teamcloud (system)
                    // properties. providers only care if the user is an admin, so we check if
                    // the user is an admin, and if so, we send the providers teamcloud user created
                    // commands (or project update connamds depending on the provider's mode)

                    if (user.IsAdmin())
                    {
                        var projects = await functionContext
                            .ListProjectsAsync()
                            .ConfigureAwait(true);

                        // TODO: change this
                        foreach (var project in projects)
                        {
                            var projectUpdateCommand = new OrchestratorProjectUpdateCommand(command.User, project);

                            functionContext.StartNewOrchestration(nameof(OrchestratorProjectUpdateCommandOrchestration), projectUpdateCommand);
                        }
                    }

                    commandResult.Result = user;

                    functionContext.SetCustomStatus($"User created.", log);
                }
                catch (Exception ex)
                {
                    functionContext.SetCustomStatus("Failed to create user.", log, ex);

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
