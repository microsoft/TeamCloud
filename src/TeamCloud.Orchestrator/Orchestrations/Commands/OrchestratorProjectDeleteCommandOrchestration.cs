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

            var project = command.Payload;

            try
            {
                functionContext.SetCustomStatus("Sending commands", log);

                var providerResults = await functionContext
                    .SendCommandAsync<ProviderProjectDeleteCommand, ProviderProjectDeleteCommandResult>(new ProviderProjectDeleteCommand(command.User, project, command.CommandId))
                    .ConfigureAwait(true);


                functionContext.SetCustomStatus("Deleting resources", log);

                var tasks = new List<Task>();

                if (!string.IsNullOrEmpty(project?.ResourceGroup?.ResourceGroupId))
                    tasks.Add(functionContext.ResetResourceGroupAsync(project.ResourceGroup.ResourceGroupId));

                if (!string.IsNullOrEmpty(project?.KeyVault?.VaultId))
                    tasks.Add(functionContext.ResetResourceGroupAsync(project.KeyVault.VaultId));

                await Task
                    .WhenAll(tasks)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus("Deleting resource groups", log);

                await functionContext
                    .CallActivityWithRetryAsync(nameof(ProjectResourcesDeleteActivity), project)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus("Deleting project", log);

                await functionContext
                    .CallActivityWithRetryAsync(nameof(ProjectDeleteActivity), project)
                    .ConfigureAwait(true);

                commandResult.Result = project;
            }
            catch (Exception ex)
            {
                functionContext.SetCustomStatus("Failed to delete project.", log, ex);

                commandResult.Errors.Add(ex);

                throw;
            }
            finally
            {
                functionContext.SetOutput(commandResult);
            }
        }
    }
}
