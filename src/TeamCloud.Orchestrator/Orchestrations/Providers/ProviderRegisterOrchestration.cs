/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Orchestrations.Projects.Activities;
using TeamCloud.Orchestrator.Orchestrations.Providers.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Providers
{
    [EternalOrchestration(EternalInstanceId, RunOnStart = true)]
    public static class ProviderRegisterOrchestration
    {
        public const string EternalInstanceId = "c0ed8d5a-ca7a-4186-84bd-062a8bac0d3a";

        [FunctionName(nameof(ProviderRegisterOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var isEternalInstance = functionContext.InstanceId.Equals(EternalInstanceId, StringComparison.OrdinalIgnoreCase);
            var nextRegistration = functionContext.CurrentUtcDateTime.NextHour(); // default set to the next full hour

            try
            {
                var provider = functionContext.GetInput<Provider>();

                functionContext.SetCustomStatus($"Registering providers {provider?.Id ?? "ALL"} ...", log);

                var systemUser = await functionContext
                    .CallActivityWithRetryAsync<User>(nameof(SystemUserActivity), null)
                    .ConfigureAwait(true);

                TeamCloudInstance teamCloud = null;
                IEnumerable<Provider> providers = null;

                if (provider is null)
                {
                    teamCloud = await functionContext
                        .CallActivityWithRetryAsync<TeamCloudInstance>(nameof(TeamCloudGetActivity), null)
                        .ConfigureAwait(true);

                    providers = teamCloud.Providers;
                }
                else
                {
                    providers = Enumerable.Repeat(provider, 1);
                }

                var registerCommandTasks = providers
                    .Select(async provider =>
                    {
                        var command = new ProviderRegisterCommand(Guid.NewGuid(), systemUser, new ProviderConfiguration
                        {
                            Properties = provider.Properties
                        });

                        var commandResult = await functionContext
                            .SendCommandAsync<ProviderRegisterCommandResult>(command, provider)
                            .ConfigureAwait(true);

                        if (commandResult?.Result is null)
                        {
                            throw new NullReferenceException($"Provider '{provider.Id}' registration failed - command result is NULL");
                        }
                        else
                        {
                            teamCloud ??= await functionContext
                                .CallActivityWithRetryAsync<TeamCloudInstance>(nameof(TeamCloudGetActivity), null)
                                .ConfigureAwait(true);

                            using (await functionContext.LockAsync(teamCloud).ConfigureAwait(true))
                            {
                                // we need to re-get the current TeamCloud instance
                                // to ensure we update the latest version inside
                                // our critical section - the same is valid for the provider

                                teamCloud = await functionContext
                                    .CallActivityWithRetryAsync<TeamCloudInstance>(nameof(TeamCloudGetActivity), null)
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

                                    await functionContext
                                        .CallActivityWithRetryAsync(nameof(TeamCloudSetActivity), teamCloud)
                                        .ConfigureAwait(true);

                                    functionContext.SetCustomStatus($"Provider '{provider.Id}' registration succeeded", log);
                                }
                            }
                        }
                    });

                await Task
                    .WhenAll(registerCommandTasks)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus($"Provider registration succeeded", log);
            }
            catch (Exception exc)
            {
                // in case of an exception we will switch to a shorter
                // registration cycle to handle hiccups on the provider side

                nextRegistration = functionContext.CurrentUtcDateTime.AddMinutes(5);

                functionContext.SetCustomStatus($"Provider registration failed", log, exc);

                if (!isEternalInstance)
                    throw; // for non-eternal instances we bubble the catched exception
            }
            finally
            {
                // there is no way to define an orchestration as "eternal only"
                // to ensure that only one eternal version exists we need to
                // compare the current instance id with the eternal instance id
                // and only reschedule if they are equal

                if (isEternalInstance)
                {
                    // there is a chance that our next registration cycle
                    // is schedule in the past. we need to ensure that this
                    // doesn't happen to avoid issues on the eternal orchestration.

                    if (nextRegistration < functionContext.CurrentUtcDateTime)
                        nextRegistration = nextRegistration.NextHour();

                    log.LogInformation($"Provider registration scheduled for {nextRegistration}");

                    await functionContext
                        .CreateTimer(nextRegistration, CancellationToken.None)
                        .ConfigureAwait(true);

                    functionContext.ContinueAsNew(null);
                }
            }


        }
    }
}
