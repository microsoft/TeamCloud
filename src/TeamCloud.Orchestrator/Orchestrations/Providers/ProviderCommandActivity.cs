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
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Orchestrations.Providers
{
    public class ProviderCommandActivity
    {
        [FunctionName(nameof(ProviderCommandActivity))]
        public async Task<ProviderCommandResult> Run(
            [ActivityTrigger] ProviderCommand activityRequest,
            ILogger log)
        {
            if (activityRequest is null) throw new ArgumentNullException(nameof(activityRequest));

            var activityResponse = new ProviderCommandResult();

            try
            {
                var providerResponse = await activityRequest.Provider.Url
                    .AppendPathSegment("api/command")
                    .WithHeader("x-functions-key", activityRequest.Provider.AuthCode)
                    .WithHeader("x-functions-callback", activityRequest.CallbackUrl)
                    .PostJsonAsync(activityRequest.Command)
                    .ConfigureAwait(false);

                if (providerResponse.StatusCode == HttpStatusCode.OK)
                {
                    var providerResponseJson = await providerResponse.Content
                        .ReadAsStringAsync()
                        .ConfigureAwait(false);

                    activityResponse.CommandResult = JsonConvert.DeserializeObject<ICommandResult>(providerResponseJson);
                }
            }
            catch (Exception exc)
            {
                activityResponse.Error = exc.Message;
            }

            return activityResponse;
        }
    }
}
