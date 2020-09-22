/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Resources;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Internal;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Activities;
using TeamCloud.Orchestrator.Orchestrations.Utilities;

namespace TeamCloud.Orchestrator.Orchestrations.Commands
{
    public static class OrchestratorProjectDeleteCommandOrchestration
    {
        [FunctionName(nameof(OrchestratorProjectDeleteCommandOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,
            ILogger log)
        {
            if (orchestrationContext is null)
                throw new ArgumentNullException(nameof(orchestrationContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var command = orchestrationContext.GetInput<OrchestratorProjectDeleteCommand>();
            var commandResult = command.CreateResult();

            using (log.BeginCommandScope(command))
            {
                orchestrationContext.SetCustomStatus($"Refreshing project", log);

                var project = commandResult.Result = (await orchestrationContext
                    .GetProjectAsync(command.ProjectId, allowUnsafe: true)
                    .ConfigureAwait(true)) ?? command.Payload;

                try
                {
                    try
                    {
                        orchestrationContext.SetCustomStatus("Sending commands", log);

                        var providerCommand = new ProviderProjectDeleteCommand
                        (
                            command.User.PopulateExternalModel(),
                            project.PopulateExternalModel(),
                            command.CommandId
                        );

                        var providerResults = await orchestrationContext
                            .SendProviderCommandAsync<ProviderProjectDeleteCommand, ProviderProjectDeleteCommandResult>(providerCommand, project)
                            .ConfigureAwait(true);
                    }
                    finally
                    {
                        orchestrationContext.SetCustomStatus("Deleting project", log);

                        await orchestrationContext
                            .DeleteProjectAsync(project)
                            .ConfigureAwait(true);

                        orchestrationContext.SetCustomStatus($"Deleting project identity", log);

                        await orchestrationContext
                            .CallActivityWithRetryAsync(nameof(ProjectIdentityDeleteActivity), project)
                            .ConfigureAwait(true);

                        orchestrationContext.SetCustomStatus("Deleting resources", log);

                        await orchestrationContext.DeleteResourcesAsync
                        (
                            false, // we are not going to wait for this operation
                            GetResourceGroupId(project?.ResourceGroup?.Id)
                        )
                        .ConfigureAwait(true);
                    }
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

        private static string GetResourceGroupId(string resourceId)
        {
            if (AzureResourceIdentifier.TryParse(resourceId, out var resourceGroupIdentifier))
                return resourceGroupIdentifier.ToString(AzureResourceSegment.ResourceGroup);

            return null;
        }
    }
}
