/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Orchestrations.Commands.Activities
{
    public static class CommandCallbackActivity
    {
        [FunctionName(nameof(CommandCallbackActivity))]
        [RetryOptions(3)]
        public static async Task<string> RunActivity(
            [ActivityTrigger] (string instanceId, ICommand command) input,
            ILogger log)
        {
            var callbackUrl = await CallbackTrigger
                .GetCallbackUrlAsync(input.instanceId, input.command)
                .ConfigureAwait(false);

            return callbackUrl;
        }
    }
}
