using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities.Activities
{
    public static class ProjectCommandMonitoringActivity
    {
        [FunctionName(nameof(ProjectCommandMonitoringActivity))]
        public static async Task<bool> RunActivity(
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

            return true; // orchstration is active (has not reached final state)
        }
    }
}
