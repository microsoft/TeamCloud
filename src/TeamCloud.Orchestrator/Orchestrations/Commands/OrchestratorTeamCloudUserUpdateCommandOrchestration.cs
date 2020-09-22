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
    public static class OrchestratorTeamCloudUserUpdateCommandOrchestration
    {
        [FunctionName(nameof(OrchestratorTeamCloudUserUpdateCommandOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,
            ILogger log)
        {
            if (orchestrationContext is null)
                throw new ArgumentNullException(nameof(orchestrationContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var command = orchestrationContext.GetInput<OrchestratorTeamCloudUserUpdateCommand>();
            var commandResult = command.CreateResult();
            var user = command.Payload;

            using (log.BeginCommandScope(command))
            {
                try
                {
                    orchestrationContext.SetCustomStatus($"Updating user.", log);

                    var existingUser = default(UserDocument);

                    using (await orchestrationContext.LockAsync<UserDocument>(user.Id.ToString()).ConfigureAwait(true))
                    {
                        existingUser = await orchestrationContext
                            .GetUserAsync(user.Id)
                            .ConfigureAwait(true);

                        if (existingUser is null)
                            throw new OrchestratorCommandException($"User '{user.Id}' does not exist.", command);

                        if (user.HasEqualTeamCloudInfo(existingUser))
                            throw new OrchestratorCommandException($"User '{user.Id}' TeamCloud details have not changed.", command);

                        if (!user.HasEqualMemberships(existingUser))
                            throw new OrchestratorCommandException($"User '{user.Id}' Project Memberships cannot be changed using the TeamCloudUserUpdateCommand. Project Memebership details must be changed using the ProjectUserUpdateCommand.", command);

                        user = await orchestrationContext
                            .SetUserTeamCloudInfoAsync(user)
                            .ConfigureAwait(true);
                    }

                    var projects = default(IEnumerable<ProjectDocument>);

                    // only update all projects if the updated user is an admin
                    // or the user was an admin before the update, otherwise
                    // only update member projects if user's teamcloud level properties changed
                    if (user.IsAdmin() || existingUser.IsAdmin())
                    {
                        projects = await orchestrationContext
                            .ListProjectsAsync()
                            .ConfigureAwait(true);
                    }
                    else if (user.ProjectMemberships.Any() && !user.Properties.SequenceEqual(existingUser.Properties))
                    {
                        projects = await orchestrationContext
                            .ListProjectsAsync(user.ProjectMemberships.Select(m => m.ProjectId).ToList())
                            .ConfigureAwait(true);
                    }

                    if (projects?.Any() ?? false)
                    {
                        foreach (var project in projects)
                        {
                            var projectUpdateCommand = new OrchestratorProjectUpdateCommand(command.User, project);

                            orchestrationContext.StartNewOrchestration(nameof(OrchestratorProjectUpdateCommandOrchestration), projectUpdateCommand);
                        }
                    }

                    var providerCommand = new ProviderTeamCloudUserUpdateCommand
                   (
                       command.User.PopulateExternalModel(),
                       command.Payload.PopulateExternalModel(),
                       command.CommandId
                   );

                    var providerResult = await orchestrationContext
                        .SendProviderCommandAsync<ProviderTeamCloudUserUpdateCommand, ProviderTeamCloudUserUpdateCommandResult>(providerCommand)
                        .ConfigureAwait(true);

                    providerResult.Errors.ToList().ForEach(e => commandResult.Errors.Add(e));

                    commandResult.Result = user;

                    orchestrationContext.SetCustomStatus($"User updated.", log);
                }
                catch (Exception ex)
                {
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
