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

namespace TeamCloud.Orchestrator
{
    public static class QueryTrigger
    {
        [FunctionName(nameof(QueryTrigger))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "command/{commandId:guid}")] HttpRequest httpRequest,
            [DurableClient] IDurableClient durableClient,
            string commandId
            /* ILogger log */)
        {
            if (httpRequest is null)
                throw new ArgumentNullException(nameof(httpRequest));

            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            if (commandId is null)
                throw new ArgumentNullException(nameof(commandId));


            var status = await durableClient
                .GetStatusAsync(commandId, showHistory: false, showHistoryOutput: false, showInput: true)
                .ConfigureAwait(false);

            var commandResult = status?.GetCommandResult();

            if (commandResult is null)
                return new NotFoundResult();

            return new OkObjectResult(commandResult);
        }
    }
}
