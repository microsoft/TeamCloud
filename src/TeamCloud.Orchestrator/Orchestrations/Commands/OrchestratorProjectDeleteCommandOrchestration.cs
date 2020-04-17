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
using TeamCloud.Model.Commands;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Orchestrations.Commands.Activities;
using TeamCloud.Orchestrator.Orchestrations.Utilities;
using TeamCloud.Orchestrator.Orchestrations.Utilities.Activities;

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

            var commandMessage = functionContext.GetInput<OrchestratorCommandMessage>();
            var command = (OrchestratorProjectDeleteCommand)commandMessage.Command;
            var commandResult = command.CreateResult();

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
                        tasks.Add(functionContext.ResetResourceGroupAsync(project.ResourceGroup.ResourceGroupId));

                    if (!string.IsNullOrEmpty(project?.KeyVault?.VaultId))
                        tasks.Add(functionContext.ResetResourceGroupAsync(project.KeyVault.VaultId));

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
}
