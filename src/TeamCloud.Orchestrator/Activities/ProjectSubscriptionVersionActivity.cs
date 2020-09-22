/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure.Resources;
using TeamCloud.Orchestration;

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
            [ActivityTrigger] IDurableActivityContext activitiyContext)
        {
            const string SubscriptionVersionDefault = "0.0.0.0";
            const string SubscriptionVersionTag = "TeamCloudVersion";

            if (activitiyContext is null)
                throw new ArgumentNullException(nameof(activitiyContext));

            var functionInput = activitiyContext.GetInput<Input>();

            var subscription = await azureResourceService
                .GetSubscriptionAsync(functionInput.SubscriptionId, throwIfNotExists: true)
                .ConfigureAwait(false);

            if (!string.IsNullOrEmpty(functionInput.SubscriptionVersion) && !functionInput.SubscriptionVersion.Equals(await GetCurrentSubscribtionVersionAsync().ConfigureAwait(false), StringComparison.OrdinalIgnoreCase))
            {
                await subscription
                    .SetTagAsync(SubscriptionVersionTag, functionInput.SubscriptionVersion)
                    .ConfigureAwait(false);

                return functionInput.SubscriptionVersion;
            }

            return await GetCurrentSubscribtionVersionAsync()
                .ConfigureAwait(false);

            async Task<string> GetCurrentSubscribtionVersionAsync()
            {
                var subscriptionVersionTag = await subscription
                    .GetTagAsync(SubscriptionVersionTag)
                    .ConfigureAwait(false);

                if (Version.TryParse(subscriptionVersionTag, out var version))
                    return version.ToString(4);

                return SubscriptionVersionDefault;
            }
        }

        public struct Input
        {
            public Guid SubscriptionId { get; set; }

            public string SubscriptionVersion { get; set; }
        }

    }

    internal static class ProjectSubscriptionVersionExtension
    {
        internal static async Task<Version> GetSubscriptionVersionAsync(this IDurableOrchestrationContext orchestrationContext, Guid subscriptionId)
        {
            var input = new ProjectSubscriptionVersionActivity.Input()
            {
                SubscriptionId = subscriptionId
            };

            var version = await orchestrationContext
                .CallActivityWithRetryAsync<string>(nameof(ProjectSubscriptionVersionActivity), input)
                .ConfigureAwait(true);

            return Version.Parse(version);
        }

        internal static async Task<Version> SetSubscriptionVersionAsync(this IDurableOrchestrationContext orchestrationContext, Guid subscriptionId, Version teamCloudVersion)
        {
            var input = new ProjectSubscriptionVersionActivity.Input()
            {
                SubscriptionId = subscriptionId,
                SubscriptionVersion = teamCloudVersion?.ToString(4) ?? throw new ArgumentNullException(nameof(teamCloudVersion))
            };

            var version = await orchestrationContext
                .CallActivityWithRetryAsync<string>(nameof(ProjectSubscriptionVersionActivity), input)
                .ConfigureAwait(true);

            return Version.Parse(version);
        }
    }
}
