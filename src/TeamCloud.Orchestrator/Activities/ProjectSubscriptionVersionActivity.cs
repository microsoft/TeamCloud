/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure.Resources;

namespace TeamCloud.Orchestrator.Activities
{
    public class ProjectSubscriptionVersionActivity
    {
        private readonly IAzureResourceService azureResourceService;

        public ProjectSubscriptionVersionActivity(IAzureResourceService azureResourceService)
        {
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ProjectSubscriptionVersionActivity))]
        public async Task<string> RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext)
        {
            const string SubscriptionVersionTag = "TeamCloudVersion";

            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var (subscriptionId, subscriptionVersion) = functionContext.GetInput<(Guid, string)>();

            var subscription = await azureResourceService
                .GetSubscriptionAsync(subscriptionId, throwIfNotExists: true)
                .ConfigureAwait(false);

            if (!string.IsNullOrEmpty(subscriptionVersion) && !subscriptionVersion.Equals(await GetCurrentSubscribtionVersionAsync().ConfigureAwait(false), StringComparison.OrdinalIgnoreCase))
            {
                await subscription
                    .SetTagAsync(SubscriptionVersionTag, subscriptionVersion)
                    .ConfigureAwait(false);

                return subscriptionVersion;
            }

            return await GetCurrentSubscribtionVersionAsync()
                .ConfigureAwait(false);

            async Task<string> GetCurrentSubscribtionVersionAsync()
            {
                var subscriptionVersionTag = await subscription
                    .GetTagAsync(SubscriptionVersionTag)
                    .ConfigureAwait(false);

                Version.TryParse(subscriptionVersionTag, out var version);

                return (version ?? new Version(0, 0, 0, 0)).ToString(4);
            }
        }

    }
}
