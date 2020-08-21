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
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Entities;

namespace TeamCloud.Orchestrator.Activities
{
    public class ProviderCreateActivity
    {
        private readonly IProviderRepository providersRepository;

        public ProviderCreateActivity(IProviderRepository providersRepository)
        {
            this.providersRepository = providersRepository ?? throw new ArgumentNullException(nameof(providersRepository));
        }

        [FunctionName(nameof(ProviderCreateActivity))]
        public async Task<ProviderDocument> RunActivity(
            [ActivityTrigger] ProviderDocument provider)
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
        public static Task<ProviderDocument> CreateProviderAsync(this IDurableOrchestrationContext functionContext, ProviderDocument provider, bool allowUnsafe = false)
            => functionContext.IsLockedBy<ProviderDocument>(provider.Id) || allowUnsafe
            ? functionContext.CallActivityWithRetryAsync<ProviderDocument>(nameof(ProviderCreateActivity), provider)
            : throw new NotSupportedException($"Unable to create provider '{provider.Id}' without acquired lock");
    }
}
