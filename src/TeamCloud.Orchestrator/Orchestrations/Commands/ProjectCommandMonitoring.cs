/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;


namespace TeamCloud.Orchestrator.Orchestrations.Commands
{
    public static class ProjectCommandMonitoring
    {
        [FunctionName(nameof(ProjectCommandMonitoringOrchestrator))]
        public static async Task ProjectCommandMonitoringOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));

            var notification = context.GetInput<ProjectCommandNotification>();

            await context
                .CreateTimer(context.CurrentUtcDateTime.AddSeconds(5), CancellationToken.None)
                .ConfigureAwait(true);

            var active = await context
                .CallActivityAsync<bool>(nameof(ProjectCommandMonitoringActivity), notification)
                .ConfigureAwait(true);

            if (active)
            {
                // monitored orchestration still active - keep monitoring
                context.ContinueAsNew(notification);
            }
        }


        [FunctionName(nameof(ProjectCommandMonitoringActivity))]
        public static async Task<bool> ProjectCommandMonitoringActivity(
            [ActivityTrigger] ProjectCommandNotification notification,
            [DurableClient] IDurableClient durableClient)
        {
            if (notification is null) throw new ArgumentNullException(nameof(notification));

            var status = await durableClient
                .GetStatusAsync(notification.CorrelationId.ToString())
                .ConfigureAwait(false);

            if (status?.IsFinalRuntimeStatus() ?? true)
            {
                // no status available or final status reached

                await durableClient
                    .RaiseEventAsync(notification.InstanceId, notification.CorrelationId.ToString())
                    .ConfigureAwait(true);

                return false; // orchestration is not active
            }
            else
            {
                // orchestation has not reached final state

                return true; // orchstration is active
            }
        }
    }
}