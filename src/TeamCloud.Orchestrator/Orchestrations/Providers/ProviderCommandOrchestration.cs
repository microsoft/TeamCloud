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
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Orchestrations.Providers.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Providers
{
    public static class ProviderCommandOrchestration
    {
        [FunctionName(nameof(ProviderCommandOrchestration))]
        public static async Task<ICommandResult> Run(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var (provider, command) = functionContext.GetInput<(Provider, ICommand)>();

            var eventName = GetExternalEventName(command.CommandId.ToString(), command.ProviderId);

            var callbackUrl = await CallbackTrigger
                .GetCallbackUrlAsync(functionContext.InstanceId, eventName)
                .ConfigureAwait(true);

            var message = new ProviderCommandMessage(command, callbackUrl);

            try
            {
                var commandResult = await functionContext
                    .CallActivityAsync<ICommandResult>(nameof(ProviderCommandActivity), (provider, message))
                    .ConfigureAwait(true);

                if (commandResult.RuntimeStatus.IsRunning())
                {
                    log.LogInformation($"Waiting for external event {eventName} in orchestration {functionContext.InstanceId}");

                    commandResult = await functionContext
                        .WaitForExternalEvent<ICommandResult>(eventName, TimeSpan.FromMinutes(30), null)
                        .ConfigureAwait(true);


                }

                return commandResult;
            }
            catch (FunctionFailedException ex)
            {
                var commandResult = command.CreateResult();
                commandResult.Exceptions.Add(ex);
                return commandResult;
            }
        }

        private static string GetExternalEventName(string commandId, string providerId)
        {
            if (commandId is null)
                throw new ArgumentNullException(nameof(commandId));

            if (providerId is null)
                throw new ArgumentNullException(nameof(providerId));

            return $"{commandId}~{providerId}";
        }
    }
}
