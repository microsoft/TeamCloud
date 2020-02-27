/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Orchestrations.Providers.Activities
{
    public static class ProviderCommandActivity
    {
        [FunctionName(nameof(ProviderCommandActivity))]
        public static async Task<ICommandResult> Run(
            [ActivityTrigger] (Provider provider, ProviderCommandMessage message) input,
            ILogger log)
        {
            if (input.provider is null)
                throw new ArgumentException($"input param must contain a valid Provider set on {nameof(input.provider)}.", nameof(input));

            if (input.message is null)
                throw new ArgumentException($"input param must contain a valid ProviderCommandMessage set on {nameof(input.message)}.", nameof(input));

            ICommandResult commandResult = input.message.Command.CreateResult();

            try
            {
                var providerUrl = new Url(input.provider.Url?.Trim());

                if (!providerUrl.Path.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    providerUrl = providerUrl.AppendPathSegment("api/command");
                }

                var response = await providerUrl
                    .WithHeader("x-functions-key", input.provider.AuthCode)
                    .WithHeader("x-functions-callback", input.message.CallbackUrl)
                    .PostJsonAsync(input.message)
                    .ConfigureAwait(false);

                var responseJson = await response.Content
                    .ReadAsStringAsync()
                    .ConfigureAwait(false);

                commandResult = JsonConvert.DeserializeObject<ICommandResult>(responseJson);
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"Sending command to provider {input.provider.Id} failed: {exc.Message}");

                commandResult.Errors.Add(exc);
            }

            return commandResult;
        }
    }
}
