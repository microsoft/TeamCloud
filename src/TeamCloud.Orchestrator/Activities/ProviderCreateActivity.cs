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
            [ActivityTrigger] IDurableActivityContext activityContext)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            var provider = activityContext.GetInput<ProviderDocument>();

            provider = await providersRepository
                .AddAsync(provider)
                .ConfigureAwait(false);

            return provider;
        }
    }

    internal static class ProviderCreateExtension
    {
        public static Task<ProviderDocument> CreateProviderAsync(this IDurableOrchestrationContext orchestrationContext, ProviderDocument provider, bool allowUnsafe = false)
            => orchestrationContext.IsLockedBy<ProviderDocument>(provider.Id) || allowUnsafe
            ? orchestrationContext.CallActivityWithRetryAsync<ProviderDocument>(nameof(ProviderCreateActivity), provider)
            : throw new NotSupportedException($"Unable to create provider '{provider.Id}' without acquired lock");
    }
}
