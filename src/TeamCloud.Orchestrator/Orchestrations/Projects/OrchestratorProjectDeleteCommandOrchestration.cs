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
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Orchestrations.Azure;
using TeamCloud.Orchestrator.Orchestrations.Projects.Activities;
using TeamCloud.Orchestrator.Orchestrations.Projects.Utilities;

namespace TeamCloud.Orchestrator.Orchestrations.Projects
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
                functionContext.SetCustomStatus("Waiting on for another project operation to complete.", log);

                await functionContext
                    .WaitForProjectCommandsAsync(command)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus("Waiting on providers to delete project resources.", log);

                var providerCommand = command is IOrchestratorCommandConvert commandConvert
                    ? commandConvert.CreateProviderCommand()
                    : throw new NotSupportedException($"Unable to convert command of type '{command.GetType()}' to '{typeof(IProviderCommand)}'");

                var providerResults = await functionContext
                    .SendCommandAsync<ICommandResult<ProviderOutput>>(providerCommand, project)
                    .ConfigureAwait(true);

                if (project.ResourceGroup != null)
                {
                    functionContext.SetCustomStatus("Deleting Azure resource group.", log);

                    await functionContext
                        .CallActivityWithRetryAsync<AzureResourceGroup>(nameof(AzureResourceGroupDeleteActivity), project.ResourceGroup)
                        .ConfigureAwait(true);
                }

                functionContext.SetCustomStatus("Deleting project from database.", log);

                project = await functionContext
                    .CallActivityWithRetryAsync<Project>(nameof(ProjectDeleteActivity), project)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus("Project deleted.", log);

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
