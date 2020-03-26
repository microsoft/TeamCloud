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
using TeamCloud.Orchestrator.Orchestrations.Commands.Activities;
using TeamCloud.Orchestrator.Orchestrations.Utilities;

namespace TeamCloud.Orchestrator.Orchestrations.Commands
{
    public static class OrchestratorProviderRegisterCommandOrchestration
    {
        [FunctionName(nameof(OrchestratorProviderRegisterCommandOrchestration) + "-Trigger")]
        public static async Task RunTrigger(
            [TimerTrigger("0 0 * * * *", RunOnStartup = true)] TimerInfo timerInfo,
            [DurableClient] IDurableClient durableClient)
        {
            if (timerInfo is null)
                throw new ArgumentNullException(nameof(timerInfo));

            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            _ = await durableClient
                .StartNewAsync(nameof(OrchestratorProviderRegisterCommandOrchestration), Guid.NewGuid().ToString(), (default(Provider), default(ProviderRegisterCommand)))
                .ConfigureAwait(false);
        }

        [FunctionName(nameof(OrchestratorProviderRegisterCommandOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var (provider, command) = functionContext
                .GetInput<(Provider, ProviderRegisterCommand)>();

            TeamCloudInstance teamCloud = null;

            try
            {
                if (command is null)
                {
                    // no command was given !!! 
                    // restart the orchestration
                    // with a new command instance. 

                    var systemUser = await functionContext
                        .CallActivityWithRetryAsync<User>(nameof(TeamCloudUserActivity), null)
                        .ConfigureAwait(true);

                    functionContext
                        .ContinueAsNew((provider, new ProviderRegisterCommand(systemUser, new ProviderConfiguration())));
                }
                else if (provider is null)
                {
                    // no provider was given !!! 
                    // fan out registration with
                    // one orchestration per provider. 

                    functionContext.SetCustomStatus($"Register providers ...", log);

                    teamCloud = await functionContext
                        .GetTeamCloudAsync()
                        .ConfigureAwait(true);

                    var tasks = teamCloud.Providers
                        .Select(provider => functionContext.CallSubOrchestratorWithRetryAsync(nameof(OrchestratorProviderRegisterCommandOrchestration), (provider, command)));

                    await Task
                        .WhenAll(tasks)
                        .ConfigureAwait(true);
                }
                else
                {
                    teamCloud = await functionContext
                        .GetTeamCloudAsync()
                        .ConfigureAwait(true);

                    command.Payload
                        .TeamCloudApplicationInsightsKey = await functionContext
                        .GetInstrumentationKeyAsync()
                        .ConfigureAwait(true);

                    command.Payload
                        .Properties = teamCloud.Properties.Override(provider.Properties);

                    var commandResult = await functionContext
                        .SendCommandAsync<ProviderRegisterCommand, ProviderRegisterCommandResult>(command, provider)
                        .ConfigureAwait(true);

                    if (commandResult?.Result != null)
                    {
                        using (await functionContext.LockAsync(teamCloud).ConfigureAwait(true))
                        {
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
                                provider.Properties = provider.Properties.Override(commandResult.Result.Properties);

                                teamCloud = await functionContext
                                    .SetTeamCloudAsync(teamCloud)
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
