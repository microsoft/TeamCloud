/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace TeamCloud.Orchestration
{
    public static class Extensions
    {
        public static bool IsJsonSerializable(this Exception exception)
            => !(exception.GetType().GetCustomAttribute<SerializableAttribute>() is null);

        private static readonly int[] FinalRuntimeStatus = new int[]
        {
            (int) OrchestrationRuntimeStatus.Completed,
            (int) OrchestrationRuntimeStatus.Failed,
            (int) OrchestrationRuntimeStatus.Canceled,
            (int) OrchestrationRuntimeStatus.Terminated
        };


        public static bool IsFinal(this OrchestrationRuntimeStatus orchestrationRuntimeStatus)
            => FinalRuntimeStatus.Contains((int)orchestrationRuntimeStatus);

        private static readonly int[] InProgressRuntimeStatus = new int[]
        {
            (int) OrchestrationRuntimeStatus.ContinuedAsNew,
            (int) OrchestrationRuntimeStatus.Pending,
            (int) OrchestrationRuntimeStatus.Running
        };

        public static bool IsInProgress(this OrchestrationRuntimeStatus orchestrationRuntimeStatus)
            => InProgressRuntimeStatus.Contains((int)orchestrationRuntimeStatus);
    }
}
