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
            if (functionContext is null) throw new ArgumentNullException(nameof(functionContext));

            var orchestratorResponse = new ProviderCommandResult();

            try
            {
                var orchestratorRequest = functionContext.GetInput<ProviderCommand>();

                var activityResponse = await functionContext
                    .CallActivityAsync<ProviderCommandResult>(nameof(ProviderCommandActivity), new ProviderCommand
                    {
                        Command = orchestratorRequest.Command,
                        Provider = orchestratorRequest.Provider,
                        CallbackUrl = await CallbackTrigger
                            .GetCallbackUrlAsync(functionContext.InstanceId)
                            .ConfigureAwait(true)
                    })
                    .ConfigureAwait(true);

                if (activityResponse.CommandResult is null)
                {
                    log.LogInformation($"Waiting for external event in orchestration {functionContext.InstanceId}");

                    // FIXME: Change timespan back to 30 mins
                    activityResponse.CommandResult = await functionContext
                        .WaitForExternalEvent<ICommandResult>(orchestratorRequest.Command.CommandId.ToString(), TimeSpan.FromMinutes(3), null)
                        .ConfigureAwait(true);
                }

                orchestratorResponse.CommandResult = activityResponse.CommandResult;
            }
            catch (Exception exc)
            {
                orchestratorResponse.Error = exc.Message;
            }

            return orchestratorResponse;
        }
    }
}
