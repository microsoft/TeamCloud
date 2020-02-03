/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Orchestrations.Providers.Activities
{
    public class ProviderRegisterActivity
    {
        private readonly ITeamCloudRepository teamCloudRepository;

        public ProviderRegisterActivity(ITeamCloudRepository teamCloudRepository)
        {
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
        }

        [FunctionName(nameof(ProviderRegisterActivity))]
        public async Task<List<Provider>> RunActivity(
            [ActivityTrigger] List<(string providerId, ProviderRegistration registration)> providerRegistrations)
        {
            if (providerRegistrations is null)
                throw new ArgumentNullException(nameof(providerRegistrations));

            var teamCloud = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            foreach (var providerRegistration in providerRegistrations)
            {
                var provider = teamCloud.Providers.FirstOrDefault(p => p.Id == providerRegistration.providerId);

                if (provider != null)
                {
                    provider.PricipalId = providerRegistration.registration.PricipalId;

                    foreach (var property in providerRegistration.registration.Variables)
                    {
                        provider.Variables[property.Key] = property.Value;
                    }
                }
            }

            await teamCloudRepository
                .SetAsync(teamCloud)
                .ConfigureAwait(false);

            return teamCloud.Providers;
        }
    }
}
