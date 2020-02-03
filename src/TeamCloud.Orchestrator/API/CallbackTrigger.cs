/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using TeamCloud.Model.Commands;

namespace TeamCloud.Orchestrator
{
    public static class CallbackTrigger
    {
        internal static async Task<string> GetCallbackUrlAsync(string instanceId)
        {
            var hostname = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
            var hostpath = $"api/callback/{instanceId}";

            if (hostname.StartsWith("localhost", StringComparison.OrdinalIgnoreCase))
            {
                return $"http://{hostname}/{hostpath}";
            }
            else
            {
                var functionKeysJson = await $"https://{hostname}/admin/functions/{nameof(CallbackTrigger)}/keys"
                    .GetJObjectAsync()
                    .ConfigureAwait(false);

                var functionKey = functionKeysJson.SelectToken("keys[0].value")?.ToString();

                return $"https://{hostname}/{hostpath}?code={functionKey}";
            }
        }

        [FunctionName(nameof(CallbackTrigger))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "callback/{instanceId}")] ProviderCommandResult providerCommandResult,
            [DurableClient] IDurableClient durableClient,
            string instanceId
            /* ILogger log */)
        {
            if (providerCommandResult is null)
                throw new ArgumentNullException(nameof(providerCommandResult));

            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            if (instanceId is null)
                throw new ArgumentNullException(nameof(instanceId));

            await durableClient
                .RaiseEventAsync(instanceId, providerCommandResult.CommandId.ToString(), providerCommandResult)
                .ConfigureAwait(false);

            return new OkResult();
        }
    }
}
