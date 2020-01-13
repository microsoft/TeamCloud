/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace TeamCloud.Orchestrator
{
    public class QueryTrigger
    {
        [FunctionName(nameof(QueryTrigger))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "command/{commandId:guid}")] HttpRequest httpRequest,
            [DurableClient] IDurableClient durableClient,
            string commandId,
            ILogger logger)
        {
            var status = await durableClient
                .GetStatusAsync(commandId, showHistory: false, showHistoryOutput: false, showInput: false)
                .ConfigureAwait(false);

            return status is null ? (IActionResult)new NotFoundResult() : new OkObjectResult(status.GetResult());
        }
    }
}
