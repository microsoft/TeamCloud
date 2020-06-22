/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data.Core;
using TeamCloud.Model.Internal.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Activities;
using TeamCloud.Orchestrator.Entities;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities
{
    public static class ProviderRegisterOrchestration
    {
        [FunctionName(nameof(ProviderRegisterOrchestration) + "-Trigger")]
        public static async Task RunTrigger(
            [TimerTrigger("0 0 * * * *", RunOnStartup = true)] TimerInfo timerInfo,
            [DurableClient] IDurableClient durableClient)
        {
            if (timerInfo is null)
                throw new ArgumentNullException(nameof(timerInfo));

            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            _ = await durableClient
                .StartNewAsync(nameof(ProviderRegisterOrchestration), Guid.NewGuid().ToString(), (default(Provider), default(ProviderRegisterCommand)))
                .ConfigureAwait(false);
        }

        [FunctionName(nameof(ProviderRegisterOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var (provider, command) = functionContext
                .GetInput<(Provider, ProviderRegisterCommand)>();

            try
            {
                if (command is null)
                {
                    // no command was given !!!
                    // restart the orchestration
                    // with a new command instance.

                    var systemUser = await functionContext
                        .CallActivityWithRetryAsync<User>(nameof(TeamCloudSystemUserActivity), null)
                        .ConfigureAwait(true);

                    var providerCommand = new ProviderRegisterCommand
                    (
                        systemUser.PopulateExternalModel(),
                        new ProviderConfiguration()
                    );

                    functionContext
                        .ContinueAsNew((provider, providerCommand));
                }
                else if (provider is null)
                {
                    // no provider was given !!!
                    // fan out registration with
                    // one orchestration per provider.

                    functionContext.SetCustomStatus($"Register providers", log);

                    var providers = await functionContext
                        .ListProvidersAsync()
                        .ConfigureAwait(true);

                    if (providers.Any())
                    {
                        var tasks = providers
                            .Select(provider => functionContext.CallSubOrchestratorWithRetryAsync(nameof(ProviderRegisterOrchestration), (provider, command)));

                        await Task
                            .WhenAll(tasks)
                            .ConfigureAwait(true);
                    }
                }
                else
                {
                    command.Payload
                        .TeamCloudApplicationInsightsKey = await functionContext
                        .GetInstrumentationKeyAsync()
                        .ConfigureAwait(true);

                    command.Payload
                        .Properties = provider.Properties;

                    var commandResult = await functionContext
                        .SendProviderCommandAsync<ProviderRegisterCommand, ProviderRegisterCommandResult>(command, provider)
                        .ConfigureAwait(true);

                    if (commandResult?.Result != null)
                    {
                        using (await functionContext.LockContainerDocumentAsync(provider).ConfigureAwait(true))
                        {
                            provider = await functionContext
                                .GetProviderAsync(provider.Id)
                                .ConfigureAwait(true);

                            if (provider is null)
                            {
                                log.LogWarning($"Provider '{provider.Id}' registration skipped - provider no longer exists");
                            }
                            else
                            {
                                provider.PrincipalId = commandResult.Result.PrincipalId;
                                provider.CommandMode = commandResult.Result.CommandMode;
                                provider.Registered = functionContext.CurrentUtcDateTime;
                                provider.Properties = provider.Properties.Override(commandResult.Result.Properties);

                                provider = await functionContext
                                    .SetProviderAsync(provider)
                                    .ConfigureAwait(true);

                                functionContext.SetCustomStatus($"Provider '{provider.Id}' registration succeeded", log);
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                if (provider != null)
                {
                    functionContext.SetCustomStatus($"Failed to register provider '{provider.Id}' - {exc.Message}", log, exc);
                }
                else if (command != null)
                {
                    functionContext.SetCustomStatus($"Failed to register providers - {exc.Message}", log, exc);
                }
                else
                {
                    functionContext.SetCustomStatus($"Failed to initiate provider registration - {exc.Message}", log, exc);
                }
            }
        }
    }
}
