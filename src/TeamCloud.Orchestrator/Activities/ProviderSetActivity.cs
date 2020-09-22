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
    public class ProviderSetActivity
    {
        private readonly IProviderRepository providersRepository;

        public ProviderSetActivity(IProviderRepository providersRepository)
        {
            this.providersRepository = providersRepository ?? throw new ArgumentNullException(nameof(providersRepository));
        }

        [FunctionName(nameof(ProviderSetActivity))]
        public async Task<ProviderDocument> RunActivity(
            [ActivityTrigger] IDurableActivityContext activityContext)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            var provider = activityContext.GetInput<ProviderDocument>();

            var newProvider = await providersRepository
                .SetAsync(provider)
                .ConfigureAwait(false);

            return newProvider;
        }
    }

    internal static class ProviderSetExtension
    {
        public static Task<ProviderDocument> SetProviderAsync(this IDurableOrchestrationContext orchestrationContext, ProviderDocument provider, bool allowUnsafe = false)
            => orchestrationContext.IsLockedByContainerDocument(provider) || allowUnsafe
            ? orchestrationContext.CallActivityWithRetryAsync<ProviderDocument>(nameof(ProviderSetActivity), provider)
            : throw new NotSupportedException($"Unable to set provider '{provider.Id}' without acquired lock");
    }
}
