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
using TeamCloud.Orchestrator.Orchestrations.Projects.Activities;
using TeamCloud.Orchestrator.Orchestrations.Providers.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Providers
{
    public static class ProviderRegisterOrchestration
    {
        public const string InstanceId = "c0ed8d5a-ca7a-4186-84bd-062a8bac0d3a";

        [FunctionName(nameof(ProviderRegisterOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var teamCloud = await functionContext
                .CallActivityAsync<TeamCloudInstance>(nameof(TeamCloudGetActivity), null)
                .ConfigureAwait(true);

            functionContext.SetCustomStatus("Registering Providers");

            var providerCommandTasks = GetProviderRegisterCommandTasks(functionContext, teamCloud);

            var providerCommandResults = await Task
                .WhenAll(providerCommandTasks)
                .ConfigureAwait(true);

            var savedProviders = await functionContext
                .CallActivityAsync<List<Provider>>(nameof(ProviderRegisterActivity), providerCommandResults)
                .ConfigureAwait(true);

            var nextRegistration = functionContext.CurrentUtcDateTime.AddHours(1);

            functionContext.SetCustomStatus($"Providers Registered. Next registraton scheduled for {nextRegistration}");

            await functionContext
                .CreateTimer(nextRegistration, CancellationToken.None)
                .ConfigureAwait(true);

            functionContext.ContinueAsNew(savedProviders);
        }

        static IEnumerable<Task<ProviderRegisterCommandResult>> GetProviderRegisterCommandTasks(IDurableOrchestrationContext context, TeamCloudInstance teamCloud)
            => teamCloud.Providers.Select(provider => context.CallSubOrchestratorAsync<ProviderRegisterCommandResult>(
                nameof(ProviderCommandOrchestration),
                (provider, new ProviderRegisterCommand(Guid.Parse(context.InstanceId), provider.Id, null, new ProviderConfiguration
                {
                    Properties = provider.Properties,
                    TeamCloudApplicationInsightsKey = teamCloud.ApplicationInsightsKey
                }))));
    }
}
