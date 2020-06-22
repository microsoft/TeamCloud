/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Internal.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Entities;

namespace TeamCloud.Orchestrator.Activities
{
    public class ProviderCreateActivity
    {
        private readonly IProvidersRepository providersRepository;

        public ProviderCreateActivity(IProvidersRepository providersRepository)
        {
            this.providersRepository = providersRepository ?? throw new ArgumentNullException(nameof(providersRepository));
        }

        [FunctionName(nameof(ProviderCreateActivity))]
        public async Task<Provider> RunActivity(
            [ActivityTrigger] Provider provider)
        {
            if (provider is null)
                throw new ArgumentNullException(nameof(provider));

            provider = await providersRepository
                .AddAsync(provider)
                .ConfigureAwait(false);

            return provider;
        }
    }

    internal static class ProviderCreateExtension
    {
        public static Task<Provider> CreateProviderAsync(this IDurableOrchestrationContext functionContext, Provider provider, bool allowUnsafe = false)
            => functionContext.IsLockedBy<Provider>(provider.Id) || allowUnsafe
            ? functionContext.CallActivityWithRetryAsync<Provider>(nameof(ProviderCreateActivity), provider)
            : throw new NotSupportedException($"Unable to create provider '{provider.Id}' without acquired lock");
    }
}
