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
    public class ProviderDeleteActivity
    {
        private readonly IProvidersRepository providersRepository;

        public ProviderDeleteActivity(IProvidersRepository providersRepository)
        {
            this.providersRepository = providersRepository ?? throw new ArgumentNullException(nameof(providersRepository));
        }

        [FunctionName(nameof(ProviderDeleteActivity))]
        public async Task RunActivity(
            [ActivityTrigger] Provider provider)
        {
            if (provider is null)
                throw new ArgumentNullException(nameof(provider));

            _ = await providersRepository
                .RemoveAsync(provider)
                .ConfigureAwait(false);
        }
    }

    internal static class ProviderDeleteExtension
    {
        public static Task<Provider> DeleteProviderAsync(this IDurableOrchestrationContext functionContext, Provider provider, bool allowUnsafe = false)
            => functionContext.IsLockedBy<Provider>(provider.Id) || allowUnsafe
            ? functionContext.CallActivityWithRetryAsync<Provider>(nameof(ProviderDeleteActivity), provider)
            : throw new NotSupportedException($"Unable to delete provider '{provider.Id}' without acquired lock");
    }
}
