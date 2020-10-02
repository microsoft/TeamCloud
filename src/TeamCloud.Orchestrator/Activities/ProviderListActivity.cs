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
        private readonly IProviderRepository providersRepository;

        public ProviderListActivity(IProviderRepository providersRepository)
        {
            this.providersRepository = providersRepository ?? throw new ArgumentNullException(nameof(providersRepository));
        }

        [FunctionName(nameof(ProviderListActivity))]
        public async Task<IEnumerable<ProviderDocument>> RunActivity(
            [ActivityTrigger] bool includeServiceProviders)
        {
            return await providersRepository
                .ListAsync(includeServiceProviders)
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }

    public class ProviderListByIdActivity
    {
        private readonly IProviderRepository providersRepository;

        public ProviderListByIdActivity(IProviderRepository providersRepository)
        {
            this.providersRepository = providersRepository ?? throw new ArgumentNullException(nameof(providersRepository));
        }

        [FunctionName(nameof(ProviderListByIdActivity))]
        public async Task<IEnumerable<ProviderDocument>> RunActivity(
            [ActivityTrigger] IList<string> providerIds)
        {
            return await providersRepository
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
