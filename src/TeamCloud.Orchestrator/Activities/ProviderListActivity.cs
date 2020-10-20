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
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Activities
{
    public class ProviderListActivity
    {
        private readonly IProviderRepository providerRepository;

        public ProviderListActivity(IProviderRepository providerRepository)
        {
            this.providerRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
        }

        [FunctionName(nameof(ProviderListActivity))]
        public async Task<IEnumerable<ProviderDocument>> RunActivity(
            [ActivityTrigger] bool includeServiceProviders)
        {
            return await providerRepository
                .ListAsync(includeServiceProviders)
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }

    public class ProviderListByIdActivity
    {
        private readonly IProviderRepository providerRepository;

        public ProviderListByIdActivity(IProviderRepository providerRepository)
        {
            this.providerRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
        }

        [FunctionName(nameof(ProviderListByIdActivity))]
        public async Task<IEnumerable<ProviderDocument>> RunActivity(
            [ActivityTrigger] IList<string> providerIds)
        {
            return await providerRepository
                .ListAsync(providerIds)
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }


    internal static class ProviderListExtension
    {
        public static Task<IEnumerable<ProviderDocument>> ListProvidersAsync(this IDurableOrchestrationContext durableOrchestrationContext, bool includeServiceProviders = false)
            => durableOrchestrationContext.CallActivityWithRetryAsync<IEnumerable<ProviderDocument>>(nameof(ProviderListActivity), includeServiceProviders);

        public static Task<IEnumerable<ProviderDocument>> ListProvidersAsync(this IDurableOrchestrationContext durableOrchestrationContext, IList<string> providerIds)
            => durableOrchestrationContext.CallActivityWithRetryAsync<IEnumerable<ProviderDocument>>(nameof(ProviderListByIdActivity), providerIds);
    }
}
