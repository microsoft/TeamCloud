/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Orchestrations.Utilities.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities
{
    public static class ProjectCommandMonitoringOrchestrator
    {
        [FunctionName(nameof(ProjectCommandMonitoringOrchestrator))]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var notification = functionContext.GetInput<ProjectCommandNotification>();

            await functionContext
                .CreateTimer(functionContext.CurrentUtcDateTime.AddSeconds(5), CancellationToken.None)
                .ConfigureAwait(true);

            var active = await functionContext
                .CallActivityWithRetryAsync<bool>(nameof(ProjectCommandMonitoringActivity), notification)
                .ConfigureAwait(true);

            if (active)
            {
                // monitored orchestration still active - keep monitoring
                functionContext.ContinueAsNew(notification);
            }
        }



    }
}
