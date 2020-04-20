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

namespace TeamCloud.Orchestrator.Orchestrations.Utilities.Activities
{
    public static class CallbackInvalidateActivity
    {
        [FunctionName(nameof(CallbackInvalidateActivity))]
        public static async Task RunActivity(
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
                    await CallbackTrigger
                         .InvalidateCallbackUrlAsync(instanceId)
                         .ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    log.LogWarning(exc, $"Failed to invlidate callback url for instance '{instanceId}' of command {command.GetType().Name} ({command.CommandId}): {exc.Message}");

                    // we are not going to bubble this exception as it doesn't affect the command processing directly. 
                    // TODO: find a good way to handle this case from a security perspective as it leaves a apikey active in a worst case

                    // throw exc.AsSerializable(); 
                }
            }
        }
    }
}
