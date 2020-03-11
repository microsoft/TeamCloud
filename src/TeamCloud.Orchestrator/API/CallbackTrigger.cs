/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TeamCloud.Http;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator
{
    public static class CallbackTrigger
    {
        private static string GetHostBaseUrl()
        {
            var hostScheme = "http";
            var hostName = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");

            if (!hostName.StartsWith("localhost", StringComparison.OrdinalIgnoreCase))
                hostScheme += "s";

            return $"{hostScheme}://{hostName}";
        }

        private static async Task<string> GetCallbackToken(string instanceId)
        {
            var hostBase = GetHostBaseUrl();

            var functionKeysUrl = hostBase
                .AppendPathSegment("admin/functions")
                .AppendPathSegment(nameof(CallbackTrigger))
                .AppendPathSegment("keys");

            var functionKeysJson = await functionKeysUrl
                .GetJObjectAsync()
                .ConfigureAwait(false);

            return functionKeysJson
                .SelectToken($"$.keys[?(@.name == '{instanceId}')].value")?
                .ToString();
        }

        internal static async Task<string> AcquireCallbackUrlAsync(string instanceId, ICommand command)
        {
            var hostBase = GetHostBaseUrl();

            var functionKey = await GetCallbackToken(instanceId)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(functionKey))
            {
                var response = await hostBase
                    .AppendPathSegment("admin/functions")
                    .AppendPathSegment(nameof(CallbackTrigger))
                    .AppendPathSegment("keys")
                    .AppendPathSegment(instanceId, true)
                    .PostJsonAsync(null)
                    .ConfigureAwait(false);

                var functionKeysJson = await response.Content
                    .ReadAsJsonAsync()
                    .ConfigureAwait(false);

                functionKey = functionKeysJson
                    .SelectToken($"$.value")?
                    .ToString();
            }

            return hostBase
                .AppendPathSegment("api/callback")
                .AppendPathSegment(instanceId, true)
                .AppendPathSegment(command.CommandId)
                .ToString();
        }

        internal static async Task InvalidateCallbackUrlAsync(string instanceId)
        {
            var hostBase = GetHostBaseUrl();

            var functionKeysUrl = hostBase
                .AppendPathSegment("admin/functions")
                .AppendPathSegment(nameof(CallbackTrigger))
                .AppendPathSegment("keys")
                .AppendPathSegment(instanceId, true);

            try
            {
                _ = await functionKeysUrl
                    .AllowAnyHttpStatus()
                    .DeleteAsync()
                    .ConfigureAwait(false);
            }
            catch
            {
                var functionKey = await GetCallbackToken(instanceId)
                    .ConfigureAwait(false);

                if (!string.IsNullOrEmpty(functionKey))
                    throw; 
            }
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
            catch (Exception exc)
            {
                log.LogError(exc, $"Callback '{instanceId}' ({eventName}) - Failed to deserialize callback payload");

                return new BadRequestResult();
            }

            try
            {
                await durableClient
                    .RaiseEventAsync(instanceId, eventName, commandResult)
                    .ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                var status = await durableClient
                    .GetStatusAsync(instanceId)
                    .ConfigureAwait(false);

                if (status is null)
                {
                    log.LogError($"Callback '{instanceId}' ({eventName}) - Orchestration instance does not exist.");

                    return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
                }
                else if (status.IsFinalRuntimeStatus())
                {
                    log.LogError($"Callback '{instanceId}' ({eventName}) - Orchestration instance already reached final state '{status.RuntimeStatus}'.");

                    return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
                }
                else
                {
                    log.LogError(exc, $"Callback '{instanceId}' ({eventName}) - Failed to raise event with payload {JsonConvert.SerializeObject(commandResult)}.");

                    throw; // re-throw exception to return an internal server error
                }
            }

            return new OkResult();
        }

        private class RetryAfterResult : IActionResult
        {
            public Task ExecuteResultAsync(ActionContext context)
            {
                throw new NotImplementedException();
            }
        }

    }
}
