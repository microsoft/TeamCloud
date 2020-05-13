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

        public static Task RaiseEventAsync(this IDurableOrchestrationContext functionContext, string instanceId, string eventName, object eventData = default)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            if (string.IsNullOrEmpty(instanceId))
                throw new ArgumentException($"Argument '{nameof(instanceId)}' must not NULL or EMPTY", nameof(instanceId));

            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentException($"Argument '{nameof(instanceId)}' must not NULL or EMPTY", nameof(eventName));

            return functionContext
                .CallActivityWithRetryAsync(nameof(RaiseEventActivity), (instanceId, eventName, eventData));
        }

#pragma warning restore CA1030 // Use events where appropriate

    }
}
