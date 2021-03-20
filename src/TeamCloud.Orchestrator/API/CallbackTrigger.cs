/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TeamCloud.Http;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator
{
    public static class CallbackTrigger
    {
        private static async Task<string> GetTokenAsync()
        {
            var masterKey = await FunctionsEnvironment
                .GetAdminKeyAsync()
                .ConfigureAwait(false);

            var hostUrl = await FunctionsEnvironment
                .GetHostUrlAsync()
                .ConfigureAwait(false);

            var json = await hostUrl
                .AppendPathSegment("admin/functions")
                .AppendPathSegment(nameof(CallbackTrigger))
                .AppendPathSegment("keys")
                .SetQueryParam("code", masterKey)
                .GetJObjectAsync()
                .ConfigureAwait(false);

            var tokens = json
                .SelectTokens($"$.keys[?(@.name != 'default')].value")
                .Select(token => token.ToString())
                .ToArray();

            if (tokens.Length == 1)
            {
                return tokens[0];
            }
            else if (tokens.Length > 1)
            {
                return tokens[new Random().Next(0, tokens.Length - 1)];
            }

            return json
                .SelectToken($"$.keys[?(@.name == 'default')].value")?
                .ToString();
        }

        internal static async Task<string> GetUrlAsync(string instanceId, ICommand command)
        {
            string functionKey = null;

            if (FunctionsEnvironment.IsAzureEnvironment)
            {
                functionKey = await GetTokenAsync().ConfigureAwait(false);

                if (string.IsNullOrEmpty(functionKey))
                    throw new NotSupportedException($"Function '{nameof(CallbackTrigger)}' must have a 'default' APIKey.");
            }

            var hostUrl = await FunctionsEnvironment
                .GetHostUrlAsync()
                .ConfigureAwait(false);

            return hostUrl
                .AppendPathSegment("api/callback")
                .AppendPathSegment(instanceId, true)
                .AppendPathSegment(command.CommandId)
                .SetQueryParam("code", functionKey)
                .ToString();
        }

        [FunctionName(nameof(CallbackTrigger))]
        public static async Task<IActionResult> RunTrigger(
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

                commandResult = TeamCloudSerialize.DeserializeObject<ICommandResult>(requestBody);
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"Callback '{instanceId}' ({eventName}) - Failed to deserialize callback payload");

                return new BadRequestResult();
            }

            try
            {
                var status = await durableClient
                    .GetStatusAsync(instanceId)
                    .ConfigureAwait(false);

                if (status?.RuntimeStatus.IsFinal() ?? true)
                {
                    // the orchestration does not exist or reached
                    // a final state - raising an external event
                    // doesn't make sense, but we need to response
                    // with an status code that indicates that
                    // another retry doesn't make sense.
                    // status code 410 (Gone) looks like a perfect fit

                    return new StatusCodeResult((int)HttpStatusCode.Gone);
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
                log.LogError(exc, $"Callback '{instanceId}' ({eventName}) - Failed to raise event with payload {JsonConvert.SerializeObject(commandResult)}.");

                throw; // re-throw exception to return an internal server error
            }

            return new OkResult();
        }
    }
}
