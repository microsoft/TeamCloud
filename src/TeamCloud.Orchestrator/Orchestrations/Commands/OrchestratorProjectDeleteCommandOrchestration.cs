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
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var command = functionContext.GetInput<OrchestratorProjectDeleteCommand>();
            var commandResult = command.CreateResult();

            using (log.BeginCommandScope(command))
            {
                functionContext.SetCustomStatus($"Refreshing project", log);

                var project = commandResult.Result = (await functionContext
                    .GetProjectAsync(command.ProjectId, allowUnsafe: true)
                    .ConfigureAwait(true)) ?? command.Payload;

                try
                {
                    try
                    {
                        functionContext.SetCustomStatus("Sending commands", log);

                        var providerCommand = new ProviderProjectDeleteCommand
                        (
                            command.User.PopulateExternalModel(),
                            project.PopulateExternalModel(),
                            command.CommandId
                        );

                        var providerResults = await functionContext
                            .SendProviderCommandAsync<ProviderProjectDeleteCommand, ProviderProjectDeleteCommandResult>(providerCommand, project)
                            .ConfigureAwait(true);
                    }
                    finally
                    {
                        functionContext.SetCustomStatus("Deleting project", log);

                        await functionContext
                            .DeleteProjectAsync(project)
                            .ConfigureAwait(true);

                        functionContext.SetCustomStatus($"Deleting project identity", log);

                        await functionContext
                            .CallActivityWithRetryAsync(nameof(ProjectIdentityDeleteActivity), project)
                            .ConfigureAwait(true);

                        functionContext.SetCustomStatus("Deleting resources", log);

                        await functionContext.DeleteResourcesAsync
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
                        functionContext.SetCustomStatus($"Command succeeded", log);
                    else
                        functionContext.SetCustomStatus($"Command failed: {commandException.Message}", log, commandException);

                    functionContext.SetOutput(commandResult);
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
