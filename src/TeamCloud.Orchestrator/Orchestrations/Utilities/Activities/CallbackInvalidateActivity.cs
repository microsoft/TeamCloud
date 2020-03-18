/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities.Activities
{
    public static class CallbackInvalidateActivity
    {
        [FunctionName(nameof(CallbackInvalidateActivity))]
        public static async Task RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var instanceId =
                functionContext.GetInput<string>();

            try
            {
                await CallbackTrigger
                     .InvalidateCallbackUrlAsync(instanceId)
                     .ConfigureAwait(false);
            }
            catch (Exception exc) when (!exc.IsSerializable(out var serializableExc))
            {
                throw serializableExc;
            }
        }
    }
}
