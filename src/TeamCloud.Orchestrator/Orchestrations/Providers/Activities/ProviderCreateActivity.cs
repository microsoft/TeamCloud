/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Orchestrations.Providers;

namespace TeamCloud.Orchestrator.Orchestrations.TeamCloud.Activities
{
    public class ProviderCreateActivity
    {
        private readonly ITeamCloudRepository teamCloudRepository;

        public ProviderCreateActivity(ITeamCloudRepository teamCloudRepository)
        {
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
        }

        [FunctionName(nameof(ProviderCreateActivity))]
        public async Task<Provider> RunActivity(
            [ActivityTrigger] Provider provider,
            [DurableClient] IDurableClient durableClient)
        {
            if (provider is null)
                throw new ArgumentNullException(nameof(provider));

            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            var teamCloud = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            teamCloud.Providers.Add(provider);

            await teamCloudRepository
                .SetAsync(teamCloud)
                .ConfigureAwait(false);

            await durableClient
                .TerminateAsync(ProviderRegisterOrchestration.InstanceId, "New Provider Added, Restarting")
                .ConfigureAwait(false);

            return provider;
        }
    }
}
