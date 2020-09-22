/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Flurl.Util;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TeamCloud.Orchestration.Eventing
{
    public static class RaiseEventActivity
    {
        [FunctionName(nameof(RaiseEventActivity))]
        public static async Task RunActivity(
            [ActivityTrigger] IDurableActivityContext activityContext,
            [DurableClient] IDurableOrchestrationClient orchestrationClient,
            ILogger log)
        {
            if (activityContext is null)
                throw new System.ArgumentNullException(nameof(activityContext));

            if (orchestrationClient is null)
                throw new System.ArgumentNullException(nameof(orchestrationClient));

            var functionInput = activityContext.GetInput<Input>();

            try
            {
                await orchestrationClient
                    .RaiseEventAsync(functionInput.InstanceId, functionInput.EventName, functionInput.EventData)
                    .ConfigureAwait(false);

                log.LogInformation($"Raised event '{functionInput.EventName}' for instance {functionInput.InstanceId}: {SerializeEventData()}");
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"Failed to raise event '{functionInput.EventName}' for instance {functionInput.InstanceId}: {exc.Message}");
            }

            string SerializeEventData()
            {
                if (functionInput.EventData is null)
                    return null;

                var eventDataType = functionInput.EventData.GetType();

                return (eventDataType.IsValueType || eventDataType.Equals(typeof(string)))
                    ? functionInput.EventData.ToInvariantString()
                    : JsonConvert.SerializeObject(functionInput.EventData);
            }
        }

        public struct Input
        {
            public string InstanceId { get; set; }

            public string EventName { get; set; }

            public object EventData { get; set; }
        }

    }
}
