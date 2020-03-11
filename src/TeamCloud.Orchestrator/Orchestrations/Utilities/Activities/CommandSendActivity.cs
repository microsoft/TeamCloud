/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Net;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities.Activities
{
    public static class CommandSendActivity
    {
        [FunctionName(nameof(CommandSendActivity))]
        [RetryOptions(3)]
        public static async Task<ICommandResult> RunActivity(
            [ActivityTrigger] (Provider provider, ProviderCommandMessage message) input,
            ILogger log)
        {
            if (input.provider is null)
                throw new ArgumentException($"input param must contain a valid Provider set on {nameof(input.provider)}.", nameof(input));

            if (input.message is null)
                throw new ArgumentException($"input param must contain a valid ProviderCommandMessage set on {nameof(input.message)}.", nameof(input));

            var providerUrl = new Url(input.provider.Url?.Trim());

            if (!providerUrl.Path.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                providerUrl = providerUrl.AppendPathSegment("api/command");
            }

            log.LogInformation($"Sending command {input.message.CommandId} ({input.message.CommandType}) to {providerUrl}. Payload:{JsonConvert.SerializeObject(input.message)}");

            try
            {
                var response = await providerUrl
                    .WithHeader("x-functions-key", input.provider.AuthCode)
                    .WithHeader("x-functions-callback", input.message.CallbackUrl)
                    .AllowHttpStatus(HttpStatusCode.Conflict)
                    .PostJsonAsync(input.message)
                    .ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    // the provider returned a conflict 
                    // this could mean that the sent command
                    // is already in-flight. lets ask the provider
                    // if there is a status available

                    response = await providerUrl
                        .AppendPathSegment(input.message.CommandId)
                        .WithHeader("x-functions-key", input.provider.AuthCode)
                        .GetAsync()
                        .ConfigureAwait(false);
                }

                var responseJson = await response.Content
                    .ReadAsStringAsync()
                    .ConfigureAwait(false);

                return JsonConvert.DeserializeObject<ICommandResult>(responseJson);
            }
            catch (Exception exc) when (!exc.IsJsonSerializable())
            {
                throw exc.EnsureJsonSerializable();
            }
        }
    }
}
