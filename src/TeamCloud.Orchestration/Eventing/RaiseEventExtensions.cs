/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace TeamCloud.Orchestration.Eventing
{
    public static class RaiseEventExtensions
    {
#pragma warning disable CA1030 // Use events where appropriate

        public static Task RaiseEventAsync(this IDurableOrchestrationContext orchestrationContext, string instanceId, string eventName, object eventData = default)
        {
            if (orchestrationContext is null)
                throw new ArgumentNullException(nameof(orchestrationContext));

            if (string.IsNullOrEmpty(instanceId))
                throw new ArgumentException($"Argument '{nameof(instanceId)}' must not NULL or EMPTY", nameof(instanceId));

            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentException($"Argument '{nameof(instanceId)}' must not NULL or EMPTY", nameof(eventName));

            return orchestrationContext
                .CallActivityWithRetryAsync(nameof(RaiseEventActivity), new RaiseEventActivity.Input()
                {
                    InstanceId = instanceId,
                    EventName = eventName,
                    EventData = eventData
                });
        }

#pragma warning restore CA1030 // Use events where appropriate

    }
}
