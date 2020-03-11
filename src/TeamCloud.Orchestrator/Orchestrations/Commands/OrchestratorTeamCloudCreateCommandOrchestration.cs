/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Orchestrations.Commands.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Commands
{
    public static class OrchestratorTeamCloudCreateCommandOrchestration
    {
        [FunctionName(nameof(OrchestratorTeamCloudCreateCommandOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext
            /* ILogger log */)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            try
            {
                var orchestratorCommand = functionContext.GetInput<OrchestratorCommandMessage>();

                var command = orchestratorCommand.Command as OrchestratorTeamCloudCreateCommand;

                var teamCloud = await functionContext
                    .CallActivityWithRetryAsync<TeamCloudInstance>(nameof(TeamCloudCreateActivity), command.Payload)
                    .ConfigureAwait(true);

                var commandResult = command.CreateResult();

                commandResult.Result = teamCloud;

                functionContext.SetOutput(commandResult);
            }
            finally
            {
                functionContext
                    .StartNewOrchestration(nameof(OrchestratorProviderRegisterCommandOrchestration), null);
            }
        }
    }
}
