/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Activities
{
    public static class CallbackUrlGetActivity
    {
        [FunctionName(nameof(CallbackUrlGetActivity))]
        [RetryOptions(3)]
        public static async Task<string> RunActivity(
            [ActivityTrigger] IDurableActivityContext activityContext,
            ILogger log)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            var functionInput = activityContext.GetInput<Input>();

            using (log.BeginCommandScope(functionInput.Command))
            {
                try
                {
                    log.LogInformation($"Acquire callback url for instance '{functionInput.InstanceId}' of command {functionInput.Command.GetType().Name} ({functionInput.Command.CommandId})");

                    var callbackUrl = await CallbackTrigger
                         .GetUrlAsync(functionInput.InstanceId, functionInput.Command)
                         .ConfigureAwait(false);

                    return callbackUrl;
                }
                catch (Exception exc)
                {
                    log.LogError(exc, $"Failed to acquire callback url for instance '{functionInput.InstanceId}' of command {functionInput.Command.GetType().Name} ({functionInput.Command.CommandId}): {exc.Message}");

                    throw exc.AsSerializable();
                }
            }
        }

        internal struct Input
        {
            public string InstanceId { get; set; }

            public ICommand Command { get; set; }
        }

    }
}
