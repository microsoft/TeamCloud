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
using TeamCloud.Orchestrator.Orchestrations.Providers;
using TeamCloud.Orchestrator.Orchestrations.TeamCloud.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.TeamCloud
{
    public static class TeamCloudCreateOrchestration
    {
        [FunctionName(nameof(TeamCloudCreateOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext
            /* ILogger log */)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var orchestratorCommand = functionContext.GetInput<OrchestratorCommandMessage>();

            var command = orchestratorCommand.Command as TeamCloudCreateCommand;

            var teamCloud = await functionContext
                .CallActivityWithRetryAsync<TeamCloudInstance>(nameof(TeamCloudCreateActivity), command.Payload)
                .ConfigureAwait(true);

            var commandResult = command.CreateResult();
            commandResult.Result = teamCloud;

            // start eternal orchestration to register providers every 1 hour
            functionContext.StartNewOrchestration(nameof(ProviderRegisterOrchestration), teamCloud.Providers, ProviderRegisterOrchestration.EternalInstanceId);

            functionContext.SetOutput(commandResult);
        }
    }
}
