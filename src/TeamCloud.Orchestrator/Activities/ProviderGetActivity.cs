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
    public class ProviderGetActivity
    {
        private readonly IProviderRepository providerRepository;

        public ProviderGetActivity(IProviderRepository providerRepository)
        {
            this.providerRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
        }

        [FunctionName(nameof(ProviderGetActivity))]
        public async Task<ProviderDocument> RunActivity(
            [ActivityTrigger] IDurableActivityContext activityContext)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            var providerId = activityContext.GetInput<string>();

            var providerDocument = await providerRepository
                .GetAsync(providerId)
                .ConfigureAwait(false);

            return providerDocument;
        }
    }

    internal static class ProviderGetExtension
    {
        public static Task<ProviderDocument> GetProviderAsync(this IDurableOrchestrationContext orchestrationContext, string providerId, bool allowUnsafe = false)
            => orchestrationContext.IsLockedBy<ProviderDocument>(providerId) || allowUnsafe
            ? orchestrationContext.CallActivityWithRetryAsync<ProviderDocument>(nameof(ProviderGetActivity), providerId)
            : throw new NotSupportedException($"Unable to get provider '{providerId}' without acquired lock");
    }
}
