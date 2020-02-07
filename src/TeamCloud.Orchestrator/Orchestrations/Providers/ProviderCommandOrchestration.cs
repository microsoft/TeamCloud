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
using TeamCloud.Orchestrator.Orchestrations.Providers.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Providers
{
    public static class ProviderCommandOrchestration
    {
        [FunctionName(nameof(ProviderCommandOrchestration))]
        public static async Task<ProviderCommandResultMessage> Run(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var providerCommandMessage = functionContext.GetInput<ProviderCommandMessage>();

            providerCommandMessage.CallbackUrl = await CallbackTrigger
                .GetCallbackUrlAsync(functionContext.InstanceId)
                .ConfigureAwait(true);

            var providerCommandResultMessage = providerCommandMessage.CreateResultMessage();

            try
            {
                providerCommandResultMessage.CommandResult = await functionContext
                    .CallActivityAsync<ICommandResult>(nameof(ProviderCommandActivity), providerCommandMessage)
                    .ConfigureAwait(true);

                if (!providerCommandResultMessage.CommandResult.RuntimeStatus.IsFinal())
                {
                    log.LogInformation($"Waiting for external event in orchestration {functionContext.InstanceId}");

                    // FIXME: Change timespan back to 30 mins
                    providerCommandResultMessage.CommandResult = await functionContext
                        .WaitForExternalEvent<ICommandResult>(providerCommandMessage.CommandId.ToString(), TimeSpan.FromMinutes(3), null)
                        .ConfigureAwait(true);


                }
            }
            catch (Exception ex)
            {
                log.LogDebug(ex, "ProviderCommandOrchestration Failded");
                providerCommandResultMessage.CommandResult.Exceptions.Add(ex);
            }

            return providerCommandResultMessage;
        }
    }
}
