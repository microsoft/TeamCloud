/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Activities
{
    internal static class ProjectSubscriptionVersionExtension
    {
        internal static async Task<Version> GetSubscriptionVersionAsync(this IDurableOrchestrationContext functionContext, Guid subscriptionId)
        {
            var version = await functionContext
                .CallActivityWithRetryAsync<string>(nameof(ProjectSubscriptionVersionActivity), (subscriptionId, default(Version)))
                .ConfigureAwait(true);

            return Version.Parse(version);
        }

        internal static async Task<Version> SetSubscriptionVersionAsync(this IDurableOrchestrationContext functionContext, Guid subscriptionId, Version teamCloudVersion)
        {
            var version = await functionContext
                .CallActivityWithRetryAsync<string>(nameof(ProjectSubscriptionVersionActivity), (subscriptionId, teamCloudVersion?.ToString(4) ?? throw new ArgumentNullException(nameof(teamCloudVersion))))
                .ConfigureAwait(true);

            return Version.Parse(version);
        }
    }
}
