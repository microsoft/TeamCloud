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
        private readonly IProviderRepository providersRepository;

        public ProviderDeleteActivity(IProviderRepository providersRepository)
        {
            this.providersRepository = providersRepository ?? throw new ArgumentNullException(nameof(providersRepository));
        }

        [FunctionName(nameof(ProviderDeleteActivity))]
        public async Task RunActivity(
            [ActivityTrigger] IDurableActivityContext activityContext)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            var provider = activityContext.GetInput<ProviderDocument>();

            _ = await providersRepository
                .RemoveAsync(provider)
                .ConfigureAwait(false);
        }
    }

    internal static class ProviderDeleteExtension
    {
        public static Task<ProviderDocument> DeleteProviderAsync(this IDurableOrchestrationContext orchestrationContext, ProviderDocument provider, bool allowUnsafe = false)
            => orchestrationContext.IsLockedBy<ProviderDocument>(provider.Id) || allowUnsafe
            ? orchestrationContext.CallActivityWithRetryAsync<ProviderDocument>(nameof(ProviderDeleteActivity), provider)
            : throw new NotSupportedException($"Unable to delete provider '{provider.Id}' without acquired lock");
    }
}
