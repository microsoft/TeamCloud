/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Internal;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Activities
{
    public static class CallbackUrlGetActivity
    {
        [FunctionName(nameof(CallbackUrlGetActivity))]
        [RetryOptions(3)]
        public static async Task<string> RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var (instanceId, command) =
                functionContext.GetInput<(string, ICommand)>();

            using (log.BeginCommandScope(command))
            {
                try
                {
                    log.LogInformation($"Acquire callback url for instance '{instanceId}' of command {command.GetType().Name} ({command.CommandId})");

                    var callbackUrl = await CallbackTrigger
                         .GetUrlAsync(instanceId, command)
                         .ConfigureAwait(false);

                    return callbackUrl;
                }
                catch (Exception exc)
                {
                    log.LogError(exc, $"Failed to acquire callback url for instance '{instanceId}' of command {command.GetType().Name} ({command.CommandId}): {exc.Message}");

                    throw exc.AsSerializable();
                }
            }
        }
    }
}
