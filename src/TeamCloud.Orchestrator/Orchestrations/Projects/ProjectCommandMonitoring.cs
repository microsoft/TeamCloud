/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace TeamCloud.Orchestrator.Orchestrations.Projects
{
    public sealed class ProjectCommandNotification
    {
        public string PendingInstanceId { get; set; }

        public Guid ActiveCommandId { get; set; }
    }


    public static class ProjectCommandMonitoring
    {
        [FunctionName(nameof(ProjectCommandMonitoringOrchestrator))]
        public static async Task ProjectCommandMonitoringOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext)
        {
            if (functionContext is null) throw new ArgumentNullException(nameof(functionContext));

            var notification = functionContext.GetInput<ProjectCommandNotification>();

            await functionContext
                .CreateTimer(functionContext.CurrentUtcDateTime.AddSeconds(5), CancellationToken.None)
                .ConfigureAwait(true);

            var active = await functionContext
                .CallActivityAsync<bool>(nameof(ProjectCommandMonitoringActivity), notification)
                .ConfigureAwait(true);

            if (active)
            {
                // monitored orchestration still active - keep monitoring
                functionContext.ContinueAsNew(notification);
            }
        }


        [FunctionName(nameof(ProjectCommandMonitoringActivity))]
        public static async Task<bool> ProjectCommandMonitoringActivity(
            [ActivityTrigger] ProjectCommandNotification notification,
            [DurableClient] IDurableClient durableClient)
        {
            if (notification is null) throw new ArgumentNullException(nameof(notification));

            var status = await durableClient
                .GetStatusAsync(notification.ActiveCommandId.ToString())
                .ConfigureAwait(false);

            if (status?.IsFinalRuntimeStatus() ?? true)
            {
                // no status available or final status reached

                await durableClient
                    .RaiseEventAsync(notification.PendingInstanceId, notification.ActiveCommandId.ToString())
                    .ConfigureAwait(true);

                return false; // orchestration is not active
            }

            // orchstration is active (has not reached final state)

            return true;
        }
    }
}