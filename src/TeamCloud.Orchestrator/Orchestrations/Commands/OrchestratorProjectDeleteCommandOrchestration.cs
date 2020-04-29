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
using TeamCloud.Model;
using TeamCloud.Model.Commands;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Activities;
using TeamCloud.Orchestrator.Entities;
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
            using (await functionContext.LockAsync(command.Payload).ConfigureAwait(true))
            {
                functionContext.SetCustomStatus($"Refreshing project", log);

                var project = commandResult.Result = (await functionContext
                    .GetProjectAsync(command.ProjectId.GetValueOrDefault())
                    .ConfigureAwait(true)) ?? command.Payload;

                try
                {
                    try
                    {
                        functionContext.SetCustomStatus("Sending commands", log);

                        var providerResults = await functionContext
                            .SendCommandAsync<ProviderProjectDeleteCommand, ProviderProjectDeleteCommandResult>(new ProviderProjectDeleteCommand(command.User, project, command.CommandId))
                            .ConfigureAwait(true);
                    }
                    finally
                    {
                        functionContext.SetCustomStatus("Deleting project", log);

                        await functionContext
                            .CallActivityWithRetryAsync(nameof(ProjectDeleteActivity), project)
                            .ConfigureAwait(true);

                        functionContext.SetCustomStatus("Deleting resources", log);

                        await functionContext.DeleteResourcesAsync
                        (
                            false, // we are not going to wait for this operation
                            GetResourceGroupId(project?.ResourceGroup?.ResourceGroupId),
                            GetResourceGroupId(project?.KeyVault?.VaultId)
                        )
                        .ConfigureAwait(true);
                    }
                }
                catch (Exception exc)
                {
                    commandResult ??= command.CreateResult();
                    commandResult.Errors.Add(exc);

                    throw;
                }
                finally
                {
                    var commandException = commandResult.GetException();

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
