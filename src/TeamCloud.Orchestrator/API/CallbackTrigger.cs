/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.IO;
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
        internal static async Task<string> GetCallbackUrlAsync(string instanceId, string eventName)
        {
            var hostname = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
            var hostpath = $"api/callback/{instanceId}/{eventName}";

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
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "callback/{instanceId}/{eventName}")] HttpRequest httpRequest,
            [DurableClient] IDurableClient durableClient,
            string instanceId,
            string eventName
            /* ILogger log */)
        {
            if (httpRequest is null)
                throw new ArgumentNullException(nameof(httpRequest));

            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            if (instanceId is null)
                throw new ArgumentNullException(nameof(instanceId));

            if (eventName is null)
                throw new ArgumentNullException(nameof(eventName));

            using var reader = new StreamReader(httpRequest.Body);

            var requestBody = await reader
                .ReadToEndAsync()
                .ConfigureAwait(false);

            var commandResult = JsonConvert.DeserializeObject<ICommandResult>(requestBody);

            await durableClient
                .RaiseEventAsync(instanceId, eventName, commandResult)
                .ConfigureAwait(false);

            return new OkResult();
        }
    }
}
