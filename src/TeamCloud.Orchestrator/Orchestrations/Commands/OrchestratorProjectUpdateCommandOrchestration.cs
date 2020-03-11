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
using TeamCloud.Orchestrator.Orchestrations.Commands.Activities;
using TeamCloud.Orchestrator.Orchestrations.Utilities;

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

            var commandMessage = functionContext.GetInput<OrchestratorCommandMessage>();

            var command = (OrchestratorProjectUpdateCommand)commandMessage.Command;
            var commandResult = command.CreateResult();

            var project = command.Payload;

            try
            {
                functionContext.SetCustomStatus("Waiting on for another project operation to complete.", log);

                await functionContext
                    .WaitForProjectCommandsAsync(command)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus($"Updating project.", log);

                project = await functionContext
                    .CallActivityWithRetryAsync<Project>(nameof(ProjectUpdateActivity), project)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus("Waiting on providers to update project resources.", log);

                var providerCommand = command is IOrchestratorCommandConvert commandConvert
                    ? commandConvert.CreateProviderCommand()
                    : throw new NotSupportedException($"Unable to convert command of type '{command.GetType()}' to '{typeof(IProviderCommand)}'");

                var providerResults = await functionContext
                    .SendCommandAsync<ICommandResult<ProviderOutput>>(providerCommand, project)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus("Project updated.", log);

                commandResult.Result = project;
            }
            catch (Exception ex)
            {
                functionContext.SetCustomStatus("Failed to update project.", log, ex);

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
