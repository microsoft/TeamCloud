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
    public class ProviderSetActivity
    {
        private readonly IProvidersRepository providersRepository;

        public ProviderSetActivity(IProvidersRepository providersRepository)
        {
            this.providersRepository = providersRepository ?? throw new ArgumentNullException(nameof(providersRepository));
        }

        [FunctionName(nameof(ProviderSetActivity))]
        public async Task<ProviderDocument> RunActivity(
            [ActivityTrigger] ProviderDocument provider)
        {
            if (provider is null)
                throw new ArgumentNullException(nameof(provider));

            var newProvider = await providersRepository
                .SetAsync(provider)
                .ConfigureAwait(false);

            return newProvider;
        }
    }

    internal static class ProviderSetExtension
    {
        public static Task<ProviderDocument> SetProviderAsync(this IDurableOrchestrationContext functionContext, ProviderDocument provider, bool allowUnsafe = false)
            => functionContext.IsLockedByContainerDocument(provider) || allowUnsafe
            ? functionContext.CallActivityWithRetryAsync<ProviderDocument>(nameof(ProviderSetActivity), provider)
            : throw new NotSupportedException($"Unable to set provider '{provider.Id}' without acquired lock");
    }
}
