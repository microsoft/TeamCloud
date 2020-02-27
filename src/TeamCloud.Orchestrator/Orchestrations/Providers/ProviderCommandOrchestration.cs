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
using TeamCloud.Orchestrator.Orchestrations.Azure;
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

            try
            {
                if (!(command is ProviderRegisterCommand) && !provider.Registered.HasValue)
                {
                    // the provider to use wasn't registered yet
                    // to ensure we have a registered provider
                    // to process our command we need to start
                    // a provider registration sub orchestration

                    await functionContext
                        .CallSubOrchestratorAsync(nameof(ProviderRegisterOrchestration), provider)
                        .ConfigureAwait(true);

                }

                if (command.ProjectId.HasValue && provider.PrincipalId.HasValue)
                {
                    await functionContext
                        .CallActivityAsync(nameof(AzureResourceGroupContributorActivity), (command.ProjectId.Value, provider.PrincipalId.Value))
                        .ConfigureAwait(true);
                }

                var eventName = GetExternalEventName(command.CommandId.ToString(), command.ProviderId);

                var callbackUrl = await CallbackTrigger
                    .GetCallbackUrlAsync(functionContext.InstanceId, eventName)
                    .ConfigureAwait(true);

                var commandMessage = new ProviderCommandMessage(command, callbackUrl);

                var commandResult = await functionContext
                    .CallActivityAsync<ICommandResult>(nameof(ProviderCommandActivity), (provider, commandMessage))
                    .ConfigureAwait(true);

                if (commandResult.RuntimeStatus.IsRunning())
                {
                    functionContext.SetCustomStatus($"Waiting for external event {eventName} for command {command.GetType().Name} in orchestration {functionContext.InstanceId}", log);

                    commandResult = await functionContext
                        .WaitForExternalEvent<ICommandResult>(eventName, TimeSpan.FromMinutes(30), null)
                        .ConfigureAwait(true);
                }

                return commandResult;
            }
            catch (FunctionFailedException ex)
            {
                var commandResult = command.CreateResult();

                commandResult.Errors.Add(ex);

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
