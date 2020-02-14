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

            var commandMessage = functionContext.GetInput<OrchestratorCommandMessage>();
            var command = commandMessage.Command as ProviderDeleteCommand;
            var commandResult = command.CreateResult();

            try
            {
                var provider = commandResult.Result = await functionContext
                    .CallActivityAsync<Provider>(nameof(ProviderDeleteActivity), command.Payload)
                    .ConfigureAwait(true);

                functionContext.StartNewOrchestration(nameof(ProviderRegisterOrchestration), null);
            }
            catch (Exception exc)
            {
                commandResult.Errors.Add(exc);
            }
            finally
            {
                functionContext.SetOutput(commandResult);
            }
        }
    }
}
