/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Resources;
using TeamCloud.Model;
using TeamCloud.Model.Commands;
using TeamCloud.Orchestration;
using TeamCloud.Orchestration.Auditing;
using TeamCloud.Orchestration.Deployment;
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

            var commandMessage = functionContext.GetInput<OrchestratorCommandMessage>();
            var command = (OrchestratorProjectDeleteCommand)commandMessage.Command;
            var commandResult = command.CreateResult();

            using (log.BeginCommandScope(command))
            {

                var project = commandResult.Result = command.Payload;

                try
                {
                    await functionContext.AuditAsync(command, commandResult)
                        .ConfigureAwait(true);

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
                    }

                    try
                    {
                        functionContext.SetCustomStatus("Deleting resources", log);

                        var tasks = new List<Task>();

                        if (!string.IsNullOrEmpty(project?.ResourceGroup?.ResourceGroupId))
                            tasks.Add(ResetResourceGroup(functionContext, project.ResourceGroup.ResourceGroupId));

                        if (!string.IsNullOrEmpty(project?.KeyVault?.VaultId))
                            tasks.Add(ResetResourceGroup(functionContext, project.KeyVault.VaultId));

                        await Task
                            .WhenAll(tasks)
                            .ConfigureAwait(true);
                    }
                    finally
                    {
                        functionContext.SetCustomStatus("Deleting resource groups", log);

                        await functionContext
                            .CallActivityWithRetryAsync(nameof(ProjectResourcesDeleteActivity), project)
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

                    await functionContext.AuditAsync(command, commandResult)
                        .ConfigureAwait(true);

                    functionContext.SetOutput(commandResult);
                }
            }
        }

        private static Task ResetResourceGroup(IDurableOrchestrationContext functionContext, string resourceGroupId)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            if (resourceGroupId is null)
                throw new ArgumentNullException(nameof(resourceGroupId));

            if (AzureResourceIdentifier.TryParse(resourceGroupId, out var resourceGroupIdentifier))
            {
                if (string.IsNullOrEmpty(resourceGroupIdentifier.ResourceGroup))
                    throw new ArgumentException($"Argument '{nameof(resourceGroupId)}' must contain a resource group name", nameof(resourceGroupId));

                return functionContext.GetDeploymentOutputAsync(nameof(ResourceGroupResetActivity), resourceGroupIdentifier.ToString(AzureResourceSegment.ResourceGroup));
            }

            throw new ArgumentException($"Invalid resource group Id: {resourceGroupId}", nameof(resourceGroupId));
        }
    }
}
