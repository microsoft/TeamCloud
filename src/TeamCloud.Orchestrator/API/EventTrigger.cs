using System;/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using TeamCloud.Http;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.API
{
    public static class EventTrigger
    {
        internal static async Task<string> GetUrlAsync()
        {
            var masterKey = await FunctionsEnvironment
                .GetAdminKeyAsync()
                .ConfigureAwait(false);

            var hostUrl = await FunctionsEnvironment
                .GetHostUrlAsync()
                .ConfigureAwait(false);

            var json = await hostUrl
                .AppendPathSegment("admin/host/systemkeys/eventgrid_extension")
                .SetQueryParam("code", masterKey, isEncoded: true)
                .GetJObjectAsync()
                .ConfigureAwait(false);

            return hostUrl
              .AppendPathSegment("runtime/webhooks/eventgrid")
              .SetQueryParam("functionName", nameof(EventTrigger))
              .SetQueryParam("code", json.SelectToken("value")?.ToString())
              .ToString();
        }

        [FunctionName(nameof(EventTrigger))]
        public static Task Run([EventGridTrigger] EventGridEvent eventGridEvent)
        {
            if (eventGridEvent is null)
                throw new ArgumentNullException(nameof(eventGridEvent));

            return Task.CompletedTask;
        }
    }
}
