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

namespace TeamCloud.Orchestrator.Orchestrations.Providers
{
    public static class ProviderCommandOrchestration
    {
        [FunctionName(nameof(ProviderCommandOrchestration))]
        public static async Task<ProviderCommandResult> Run(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var providerCommand = functionContext.GetInput<ProviderCommand>();

            providerCommand.CallbackUrl = await CallbackTrigger
                .GetCallbackUrlAsync(functionContext.InstanceId)
                .ConfigureAwait(true);

            var providerCommandResult = new ProviderCommandResult(providerCommand);

            try
            {
                providerCommandResult = await functionContext
                    .CallActivityAsync<ProviderCommandResult>(nameof(ProviderCommandActivity), providerCommand)
                    .ConfigureAwait(true);

                if (!providerCommandResult.RuntimeStatus.IsFinal())
                {
                    log.LogInformation($"Waiting for external event in orchestration {functionContext.InstanceId}");

                    // FIXME: Change timespan back to 30 mins
                    providerCommandResult = await functionContext
                        .WaitForExternalEvent<ProviderCommandResult>(providerCommand.CommandId.ToString(), TimeSpan.FromMinutes(3), null)
                        .ConfigureAwait(true);
                }
            }
            catch (Exception ex)
            {
                log.LogDebug(ex, "ProviderCommandOrchestration Failded");
                providerCommandResult.Error = ex.Message;
            }

            return providerCommandResult;
        }
    }
}
