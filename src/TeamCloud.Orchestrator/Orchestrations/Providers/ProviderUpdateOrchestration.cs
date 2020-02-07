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
    public static class ProviderUpdateOrchestration
    {
        [FunctionName(nameof(ProviderUpdateOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext
            /* ILogger log */)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var orchestratorCommand = functionContext.GetInput<OrchestratorCommandMessage>();

            var command = orchestratorCommand.Command as ProviderUpdateCommand;

            var provider = await functionContext
                .CallActivityAsync<Provider>(nameof(ProviderUpdateActivity), command.Payload)
                .ConfigureAwait(true);

            var commandResult = command.CreateResult();
            commandResult.Result = provider;

            functionContext.SetOutput(commandResult);

            functionContext.StartNewOrchestration(nameof(ProviderRegisterOrchestration), null, ProviderRegisterOrchestration.InstanceId);
        }
    }
}
