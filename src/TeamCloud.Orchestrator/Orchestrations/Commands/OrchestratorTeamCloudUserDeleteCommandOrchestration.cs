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
    public static class OrchestratorTeamCloudUserDeleteCommandOrchestration
    {
        [FunctionName(nameof(OrchestratorTeamCloudUserDeleteCommandOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,
            ILogger log)
        {
            if (orchestrationContext is null)
                throw new ArgumentNullException(nameof(orchestrationContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var command = orchestrationContext.GetInput<OrchestratorTeamCloudUserDeleteCommand>();
            var commandResult = command.CreateResult();
            var user = command.Payload;

            using (log.BeginCommandScope(command))
            {
                try
                {
                    orchestrationContext.SetCustomStatus($"Deleting user.", log);

                    using (await orchestrationContext.LockAsync<UserDocument>(user.Id.ToString()).ConfigureAwait(true))
                    {
                        await orchestrationContext
                            .DeleteUserAsync(user.Id)
                            .ConfigureAwait(true);
                    }

                    var projects = default(IEnumerable<ProjectDocument>);

                    // TODO: this is totally wrong
                    // only update all projects if user was an admin
                    if (user.IsAdmin())
                    {
                        projects = await orchestrationContext
                            .ListProjectsAsync()
                            .ConfigureAwait(true);
                    }
                    else if (user.ProjectMemberships.Any())
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

                    var providerCommand = new ProviderTeamCloudUserDeleteCommand
                    (
                        command.User.PopulateExternalModel(),
                        command.Payload.PopulateExternalModel(),
                        command.CommandId
                    );

                    var providerResult = await orchestrationContext
                        .SendProviderCommandAsync<ProviderTeamCloudUserDeleteCommand, ProviderTeamCloudUserDeleteCommandResult>(providerCommand)
                        .ConfigureAwait(true);

                    providerResult.Errors.ToList().ForEach(e => commandResult.Errors.Add(e));

                    commandResult.Result = user;

                    orchestrationContext.SetCustomStatus($"User deleted.", log);
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
