/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Internal;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Activities;
using TeamCloud.Orchestrator.Orchestrations.Utilities;
using TeamCloud.Orchestrator.Entities;
using System.Linq;

namespace TeamCloud.Orchestrator.Orchestrations.Commands
{
    public static class OrchestratorProjectUpdateCommandOrchestration
    {
        [FunctionName(nameof(OrchestratorProjectUpdateCommandOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,
            ILogger log)
        {
            if (orchestrationContext is null)
                throw new ArgumentNullException(nameof(orchestrationContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var command = orchestrationContext.GetInput<OrchestratorProjectUpdateCommand>();
            var commandResult = command.CreateResult();

            using (log.BeginCommandScope(command))
            {
                var project = commandResult.Result = command.Payload;

                try
                {
                    var projectUsers = project.Users.ToList();

                    using (await orchestrationContext.LockContainerDocumentAsync(project).ConfigureAwait(true))
                    {
                        orchestrationContext.SetCustomStatus($"Updating project", log);

                        project = commandResult.Result = await orchestrationContext
                            .SetProjectAsync(project)
                            .ConfigureAwait(true);

                        orchestrationContext.SetCustomStatus($"Adding users", log);

                        project.Users = await Task
                            .WhenAll(projectUsers.Select(user => orchestrationContext.SetUserProjectMembershipAsync(user, project.Id, allowUnsafe: true)))
                            .ConfigureAwait(true);
                    }

                    orchestrationContext.SetCustomStatus("Waiting on providers to update project resources.", log);

                    var providerCommand = new ProviderProjectUpdateCommand
                    (
                        command.User.PopulateExternalModel(),
                        project.PopulateExternalModel(),
                        command.CommandId
                    );

                    var providerResults = await orchestrationContext
                        .SendProviderCommandAsync<ProviderProjectUpdateCommand, ProviderProjectUpdateCommandResult>(providerCommand, project)
                        .ConfigureAwait(true);
                }
                catch (Exception exc)
                {
                    commandResult ??= command.CreateResult();
                    commandResult.Errors.Add(exc);
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
