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
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Orchestrations.Azure;
using TeamCloud.Orchestrator.Orchestrations.Commands.Activities;
using TeamCloud.Orchestrator.Orchestrations.Providers;

namespace TeamCloud.Orchestrator.Orchestrations.Commands
{
    public static class CommandSendOrchestration
    {
        [FunctionName(nameof(CommandSendOrchestration))]
        public static async Task<ICommandResult> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var (providerCommand, provider) = functionContext.GetInput<(IProviderCommand, Provider)>();

            await RegisterProviderAsync(functionContext, providerCommand, provider, log)
                .ConfigureAwait(true);

            if (providerCommand.ProjectId.HasValue && provider.PrincipalId.HasValue)
            {
                // ensure the provider we are going to
                // use has contributor access on the
                // project's resource group

                await functionContext
                    .CallActivityWithRetryAsync(nameof(AzureResourceGroupContributorActivity), (providerCommand.ProjectId.Value, provider.PrincipalId.Value))
                    .ConfigureAwait(true);
            }

            var callbackUrl = await functionContext
                .CallActivityWithRetryAsync<string>(nameof(CommandCallbackActivity), (functionContext.InstanceId, providerCommand))
                .ConfigureAwait(true);

            var commandResult = providerCommand.CreateResult();

            try
            {
                var commandMessage = new ProviderCommandMessage(providerCommand, callbackUrl);

                commandResult = await functionContext
                    .CallActivityWithRetryAsync<ICommandResult>(nameof(CommandSendActivity), (provider, commandMessage))
                    .ConfigureAwait(true);

                if (commandResult.RuntimeStatus.IsRunning())
                {
                    // the command result has no final runtime status, so we 
                    // need to wait for the final result as an external event

                    functionContext.SetCustomStatus($"Waiting for external event: Command={providerCommand.CommandId} Provider={provider.Id} Callback={callbackUrl}", log);

                    commandResult = await functionContext
                        .WaitForExternalEvent<ICommandResult>(providerCommand.CommandId.ToString(), TimeSpan.FromMinutes(30), null)
                        .ConfigureAwait(true);
                }
            }
            catch (Exception exc)
            {
                commandResult.Errors.Add(exc);
            }

            return commandResult;
        }

        private static Task RegisterProviderAsync(IDurableOrchestrationContext functionContext, IProviderCommand providerCommand, Provider provider, ILogger log)
        {
            if (providerCommand is ProviderRegisterCommand || provider.Registered.HasValue)
                return Task.CompletedTask;

            return functionContext
                .CallSubOrchestratorWithRetryAsync(nameof(OrchestratorProviderRegisterCommandOrchestration), provider);
        }
    }
}
