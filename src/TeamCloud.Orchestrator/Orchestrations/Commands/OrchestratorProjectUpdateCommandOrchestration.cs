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
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var command = functionContext.GetInput<OrchestratorProjectUpdateCommand>();
            var commandResult = command.CreateResult();

            using (log.BeginCommandScope(command))
            {
                var project = commandResult.Result = command.Payload;

                try
                {
                    var projectUsers = project.Users.ToList();

                    using (await functionContext.LockContainerDocumentAsync(project).ConfigureAwait(true))
                    {
                        functionContext.SetCustomStatus($"Updating project", log);

                        project = commandResult.Result = await functionContext
                            .SetProjectAsync(project)
                            .ConfigureAwait(true);

                        functionContext.SetCustomStatus($"Adding users", log);

                        project.Users = await Task
                            .WhenAll(projectUsers.Select(user => functionContext.SetUserProjectMembershipAsync(user, project.Id, allowUnsafe: true)))
                            .ConfigureAwait(true);
                    }

                    functionContext.SetCustomStatus("Waiting on providers to update project resources.", log);

                    var providerCommand = new ProviderProjectUpdateCommand
                    (
                        command.User.PopulateExternalModel(),
                        project.PopulateExternalModel(),
                        command.CommandId
                    );

                    var providerResults = await functionContext
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
                        functionContext.SetCustomStatus($"Command succeeded", log);
                    else
                        functionContext.SetCustomStatus($"Command failed: {commandException.Message}", log, commandException);

                    functionContext.SetOutput(commandResult);
                }
            }
        }
    }
}
