/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TeamCloud.Http;
using TeamCloud.Model.Commands;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator
{
    public static class CallbackTrigger
    {
        internal static Task<string> GetCallbackUrlAsync(string instanceId, ICommand command)
            => GetCallbackUrlAsync(instanceId, command.CommandId.ToString());

        internal static async Task<string> GetCallbackUrlAsync(string instanceId, string eventName)
        {
            var scheme = "http";
            var hostname = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
            var hostpath = $"api/callback/{HttpUtility.UrlEncode(instanceId)}/{HttpUtility.UrlEncode(eventName)}";

            if (!hostname.StartsWith("localhost", StringComparison.OrdinalIgnoreCase))
            {
                scheme += "s";
            }

            var functionKeysJson = await $"{scheme}://{hostname}/admin/functions/{nameof(CallbackTrigger)}/keys"
                .GetJObjectAsync()
                .ConfigureAwait(false);

            var functionKey = functionKeysJson.SelectToken("keys[0].value")?.ToString();

            return $"{scheme}://{hostname}/{hostpath}?code={functionKey}";
        }

        [FunctionName(nameof(CallbackTrigger))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "callback/{instanceId}/{eventName}")] HttpRequest httpRequest,
            [DurableClient] IDurableClient durableClient,
            string instanceId,
            string eventName,
            ILogger log)
        {
            if (httpRequest is null)
                throw new ArgumentNullException(nameof(httpRequest));

            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            if (instanceId is null)
                throw new ArgumentNullException(nameof(instanceId));

            if (eventName is null)
                throw new ArgumentNullException(nameof(eventName));

            ICommandResult commandResult;

            try
            {
                using var reader = new StreamReader(httpRequest.Body);

                var requestBody = await reader
                    .ReadToEndAsync()
                    .ConfigureAwait(false);

                commandResult = JsonConvert.DeserializeObject<ICommandResult>(requestBody);
            }
            catch
            {
                return new BadRequestResult();
            }

            try
            {
                var orchestrationStatus = await durableClient
                    .GetStatusAsync(instanceId)
                    .ConfigureAwait(false);

                if (orchestrationStatus?.RuntimeStatus.IsFinal() ?? true)
                {
                    // the orchestration doesn't exist or reached
                    // a final state and won't accept any events

                    return new NotFoundResult();
                }
                else
                {
                    await durableClient
                        .RaiseEventAsync(instanceId, eventName, commandResult)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception exc)
            {
                log.LogWarning(exc, $"Failed to raise event {eventName} for instance {instanceId}.");

                throw; // re-throw exception to return an internal server error
            }

            return new OkResult();
        }
    }
}
