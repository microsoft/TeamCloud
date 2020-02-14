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
    public class ProviderDeleteActivity
    {
        private readonly ITeamCloudRepository teamCloudRepository;

        public ProviderDeleteActivity(ITeamCloudRepository teamCloudRepository)
        {
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
        }

        [FunctionName(nameof(ProviderDeleteActivity))]
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

            if (teamCloud.Providers.Remove(provider))
            {
                await teamCloudRepository
                    .SetAsync(teamCloud)
                    .ConfigureAwait(false);
            }

            await durableClient
                .TerminateAsync(ProviderRegisterOrchestration.EternalInstanceId, "New Provider Added, Restarting")
                .ConfigureAwait(false);


            return provider;
        }
    }
}
