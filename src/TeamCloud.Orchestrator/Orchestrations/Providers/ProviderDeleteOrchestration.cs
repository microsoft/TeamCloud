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
using TeamCloud.Orchestrator.Orchestrations.Providers;
using TeamCloud.Orchestrator.Orchestrations.TeamCloud.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.TeamCloud
{
    public static class ProviderDeleteOrchestration
    {
        [FunctionName(nameof(ProviderDeleteOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext
            /* ILogger log */)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var orchestratorCommand = functionContext.GetInput<OrchestratorCommandMessage>();

            var command = orchestratorCommand.Command as ProviderDeleteCommand;

            var provider = await functionContext
                .CallActivityAsync<Provider>(nameof(ProviderDeleteActivity), command.Payload)
                .ConfigureAwait(true);

            var commandResult = command.CreateResult();
            commandResult.Result = provider;

            functionContext.SetOutput(commandResult);

            functionContext.StartNewOrchestration(nameof(ProviderRegisterOrchestration), null, ProviderRegisterOrchestration.InstanceId);
        }
    }
}
