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
using TeamCloud.Orchestrator.Orchestrations.Commands;
using TeamCloud.Orchestrator.Orchestrations.Commands.Activities;
using TeamCloud.Orchestrator.Orchestrations.Utilities.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities
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

            var (command, provider) = functionContext.GetInput<(IProviderCommand, Provider)>();

            var commandResult = command.CreateResult();
            var commandTimeout = TimeSpan.FromMinutes(30);

            var commandMessage = default(ICommandMessage);
            var commandCallback = default(string);

            try
            {
                functionContext.SetCustomStatus($"Acquire callback url for command '{command.CommandId}'", log);

                commandCallback = await functionContext
                    .CallActivityWithRetryAsync<string>(nameof(CallbackAcquireActivity), (functionContext.InstanceId, command))
                    .ConfigureAwait(true);

                commandMessage = new ProviderCommandMessage(command, commandCallback);

                functionContext.SetCustomStatus($"Prepare sending command '{command.CommandId}'", log);

                await RegisterProviderAsync(functionContext, command, provider, log)
                    .ConfigureAwait(true);

                await EnableProviderAsync(functionContext, command, provider, log)
                    .ConfigureAwait(true);

                await functionContext
                    .AuditAsync(provider, command, commandResult)
                    .ConfigureAwait(true);

                try
                {
                    functionContext.SetCustomStatus($"Sending command '{command.CommandId}'", log);

                    commandResult = await functionContext
                        .CallActivityWithRetryAsync<ICommandResult>(nameof(CommandSendActivity), (provider, commandMessage))
                        .ConfigureAwait(true);
                }
                finally
                {
                    await functionContext
                        .AuditAsync(provider, command, commandResult)
                        .ConfigureAwait(true);
                }

                if (commandResult.RuntimeStatus.IsRunning())
                {
                    functionContext.SetCustomStatus($"Waiting for command ({command.CommandId}) result on {commandCallback}", log);

                    commandResult = await functionContext
                        .WaitForExternalEvent<ICommandResult>(command.CommandId.ToString(), commandTimeout, null)
                        .ConfigureAwait(true);

                    if (commandResult is null)
                    {
                        commandResult = command.CreateResult();

                        throw new TimeoutException($"Provider '{provider.Id}' ran into timeout ({commandTimeout})");
                    }
                }
            }
            catch (Exception exc)
            {
                functionContext.SetCustomStatus($"Sending command '{command.CommandId}' failed: {exc.Message}", log, exc);

                commandResult.Errors.Add(exc);
            }
            finally
            {
                if (!string.IsNullOrEmpty(commandCallback))
                {
                    functionContext.SetCustomStatus($"Invalidating callback url for command '{command.CommandId}'", log);

                    await functionContext
                        .CallActivityWithRetryAsync(nameof(CallbackInvalidateActivity), functionContext.InstanceId)
                        .ConfigureAwait(true);
                }

                await functionContext
                    .AuditAsync(provider, command, commandResult)
                    .ConfigureAwait(true);
            }

            return commandResult;
        }

        private static Task RegisterProviderAsync(IDurableOrchestrationContext functionContext, IProviderCommand providerCommand, Provider provider, ILogger log)
        {
            if (providerCommand is ProviderRegisterCommand || provider.Registered.HasValue)
                return Task.CompletedTask;

            return functionContext
                .RegisterProviderAsync(provider, true);
        }

        private static Task EnableProviderAsync(IDurableOrchestrationContext functionContext, IProviderCommand providerCommand, Provider provider, ILogger log)
        {
            if (!providerCommand.ProjectId.HasValue || !provider.PrincipalId.HasValue)
                return Task.CompletedTask;

            return functionContext
                .CallActivityWithRetryAsync(nameof(AzureResourceGroupContributorActivity), (providerCommand.ProjectId.Value, provider.PrincipalId.Value));
        }
    }
}
