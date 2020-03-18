/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities.Activities
{
    public static class ProjectCommandSerializationActivity
    {
        [FunctionName(nameof(ProjectCommandSerializationActivity))]
        public static async Task<bool> RunActivity(
            [ActivityTrigger] ProjectCommandNotification notification,
            [DurableClient] IDurableClient durableClient)
        {
            if (notification is null)
                throw new ArgumentNullException(nameof(notification));

            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            var status = await durableClient
                .GetStatusAsync(notification.ActiveCommandId.ToString())
                .ConfigureAwait(false);

            if (!(status?.RuntimeStatus.IsFinal() ?? false))
            {
                _ = await durableClient
                    .StartNewAsync(nameof(ProjectCommandMonitoringOrchestrator), notification)
                    .ConfigureAwait(false);

                return true; // command monitoring started
            }

            return false; // no command monitoring started
        }
    }
}
