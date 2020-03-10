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
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Orchestrations.Providers.Activities;
using TeamCloud.Orchestrator.Orchestrations.TeamCloud.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Providers
{
    public static class OrchestratorProviderRegisterCommandOrchestration
    {
        [FunctionName(nameof(OrchestratorProviderRegisterCommandOrchestration) + "-Trigger")]
        public static async Task RunTrigger(
            [TimerTrigger("0 0 * * * *", RunOnStartup = true)] TimerInfo timerInfo,
            [DurableClient] IDurableClient durableClient,
            ILogger log)
        {
            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            _ = await durableClient
                .StartNewAsync(nameof(OrchestratorProviderRegisterCommandOrchestration))
                .ConfigureAwait(false);
        }

        [FunctionName(nameof(OrchestratorProviderRegisterCommandOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var provider = functionContext.GetInput<Provider>();

            functionContext.SetCustomStatus($"Registering providers {provider?.Id ?? "ALL"} ...", log);

            TeamCloudInstance teamCloud = null;

            if (provider is null)
            {
                teamCloud = await functionContext
                    .GetTeamCloudAsync()
                    .ConfigureAwait(true);

                var tasks = teamCloud.Providers
                    .Select(provider => functionContext.CallSubOrchestratorWithRetryAsync(nameof(OrchestratorProviderRegisterCommandOrchestration), provider));

                await Task
                    .WhenAll(tasks)
                    .ConfigureAwait(true);
            }
            else
            {
                try
                {
                    var systemUser = await functionContext
                        .CallActivityWithRetryAsync<User>(nameof(SystemUserActivity), null)
                        .ConfigureAwait(true);

                    var command = new ProviderRegisterCommand(systemUser, new ProviderConfiguration
                    {
                        Properties = provider.Properties
                    });

                    var commandResult = await functionContext
                        .SendCommandAsync<ProviderRegisterCommandResult>(command, provider)
                        .ConfigureAwait(true);

                    if (commandResult?.Result is null)
                    {
                        throw new NullReferenceException($"Provider '{provider.Id}' returned no result");
                    }
                    else
                    {
                        teamCloud ??= await functionContext
                            .GetTeamCloudAsync()
                            .ConfigureAwait(true);

                        using (await functionContext.LockAsync(teamCloud).ConfigureAwait(true))
                        {
                            // we need to re-get the current TeamCloud instance
                            // to ensure we update the latest version inside
                            // our critical section - the same is valid for the provider

                            teamCloud = await functionContext
                                .GetTeamCloudAsync()
                                .ConfigureAwait(true);

                            provider = teamCloud.Providers
                                .FirstOrDefault(p => p.Id == provider.Id);

                            if (provider is null)
                            {
                                log.LogWarning($"Provider '{provider.Id}' registration skipped - provider no longer exists");
                            }
                            else
                            {
                                provider.PrincipalId = commandResult.Result.PrincipalId;
                                provider.Registered = functionContext.CurrentUtcDateTime;
                                provider.Properties = provider.Properties.Merge(commandResult.Result.Properties);

                                teamCloud = await functionContext
                                    .SetTeamCloudAsync(teamCloud)
                                    .ConfigureAwait(true);

                                functionContext.SetCustomStatus($"Provider '{provider.Id}' registration succeeded", log);
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    functionContext.SetCustomStatus($"Provider '{provider.Id}' registration failed", log, exc);

                    if (!string.IsNullOrEmpty(functionContext.ParentInstanceId)) throw;
                }
            }
        }
    }
}
