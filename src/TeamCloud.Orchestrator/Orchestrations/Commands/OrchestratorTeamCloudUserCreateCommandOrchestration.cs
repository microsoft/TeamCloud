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
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Internal;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Activities;
using TeamCloud.Orchestrator.Entities;
using TeamCloud.Orchestrator.Orchestrations.Utilities;

namespace TeamCloud.Orchestrator.Orchestrations.Commands
{
    public static class OrchestratorTeamCloudUserCreateCommandOrchestration
    {
        [FunctionName(nameof(OrchestratorTeamCloudUserCreateCommandOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,
            ILogger log)
        {
            if (orchestrationContext is null)
                throw new ArgumentNullException(nameof(orchestrationContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var command = orchestrationContext.GetInput<OrchestratorTeamCloudUserCreateCommand>();
            var commandResult = command.CreateResult();
            var user = command.Payload;

            using (log.BeginCommandScope(command))
            {
                try
                {
                    orchestrationContext.SetCustomStatus($"Creating user.", log);

                    using (await orchestrationContext.LockContainerDocumentAsync(user).ConfigureAwait(true))
                    {
                        var existingUser = await orchestrationContext
                            .GetUserAsync(user.Id)
                            .ConfigureAwait(true);

                        if (existingUser != null)
                            throw new OrchestratorCommandException($"User '{user.Id}' already exist.", command);

                        user = await orchestrationContext
                            .SetUserTeamCloudInfoAsync(user)
                            .ConfigureAwait(true);
                    }

                    // this will only be called on a newly created user with teamcloud (system)
                    // properties. providers only care if the user is an admin, so we check if
                    // the user is an admin, and if so, we send the providers teamcloud user created
                    // commands (or project update connamds depending on the provider's mode)
                    if (user.IsAdmin())
                    {
                        var projects = await orchestrationContext
                            .ListProjectsAsync()
                            .ConfigureAwait(true);

                        // TODO: change this
                        foreach (var project in projects)
                        {
                            var projectUpdateCommand = new OrchestratorProjectUpdateCommand(command.User, project);

                            orchestrationContext.StartNewOrchestration(nameof(OrchestratorProjectUpdateCommandOrchestration), projectUpdateCommand);
                        }
                    }

                    var providerCommand = new ProviderTeamCloudUserCreateCommand
                    (
                        command.User.PopulateExternalModel(),
                        command.Payload.PopulateExternalModel(),
                        command.CommandId
                    );

                    var providerResult = await orchestrationContext
                        .SendProviderCommandAsync<ProviderTeamCloudUserCreateCommand, ProviderTeamCloudUserCreateCommandResult>(providerCommand)
                        .ConfigureAwait(true);

                    providerResult.Errors.ToList().ForEach(e => commandResult.Errors.Add(e));

                    commandResult.Result = user;

                    orchestrationContext.SetCustomStatus($"User created.", log);
                }
                catch (Exception ex)
                {
                    orchestrationContext.SetCustomStatus("Failed to create user.", log, ex);

                    commandResult ??= command.CreateResult();
                    commandResult.Errors.Add(ex);
                }
                finally
                {
                    var commandException = commandResult.Errors?.ToException();

                    if (commandException is null)
                        orchestrationContext.SetCustomStatus($"Command succeeded", log);
                    else
                        orchestrationContext.SetCustomStatus($"Command failed: {commandException.Message}", log, commandException);

                    orchestrationContext.SetOutput(commandResult);
                }
            }
        }
    }
}
