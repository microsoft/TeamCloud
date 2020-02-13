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
using TeamCloud.Model.Commands;
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
            [ActivityTrigger] List<ProviderRegisterCommandResult> providerRegistrationResults)
        {
            if (providerRegistrationResults is null)
                throw new ArgumentNullException(nameof(providerRegistrationResults));

            var teamCloud = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            foreach (var result in providerRegistrationResults)
            {
                var provider = teamCloud.Providers.FirstOrDefault(p => p.Id == result.ProviderId);

                if (provider != null)
                {
                    provider.PrincipalId = result.Result.PrincipalId;

                    foreach (var property in result.Result.Properties)
                    {
                        provider.Properties[property.Key] = property.Value;
                    }
                }
            }

            await teamCloudRepository
                .SetAsync(teamCloud)
                .ConfigureAwait(false);

            return teamCloud
                .Providers
                .ToList();
        }
    }
}
