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

namespace TeamCloud.Orchestrator.Orchestrations.Providers
{
    public static class ProviderCommandActivity
    {
        [FunctionName(nameof(ProviderCommandActivity))]
        public static async Task<ProviderCommandResult> Run(
            [ActivityTrigger] ProviderCommand providerCommand,
            ILogger log)
        {
            if (providerCommand is null)
                throw new ArgumentNullException(nameof(providerCommand));

            var providerCommandResult = new ProviderCommandResult(providerCommand);

            try
            {
                var providerResponse = await providerCommand.Provider.Url
                    .AppendPathSegment("api/command")
                    .WithHeader("x-functions-key", providerCommand.Provider.AuthCode)
                    .WithHeader("x-functions-callback", providerCommand.CallbackUrl)
                    .PostJsonAsync(providerCommand)
                    .ConfigureAwait(false);

                if (providerResponse.StatusCode == HttpStatusCode.OK)
                {
                    var providerResponseJson = await providerResponse.Content
                        .ReadAsStringAsync()
                        .ConfigureAwait(false);

                    providerCommandResult = JsonConvert.DeserializeObject<ProviderCommandResult>(providerResponseJson);
                }
            }
            catch (Exception ex)
            {
                log.LogDebug(ex, "ProviderCommandActivity Failded");
                providerCommandResult.Error = ex.Message;
            }

            return providerCommandResult;
        }
    }
}
