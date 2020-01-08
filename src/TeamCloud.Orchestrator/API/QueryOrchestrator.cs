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
    public class QueryOrchestrator
    {
        [FunctionName(nameof(QueryOrchestrator))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "orchestrator/{instanceId:guid}")] HttpRequest httpRequest,
            [DurableClient] IDurableClient durableClient,
            string instanceId,
            ILogger logger)
        {
            var status = await durableClient.GetStatusAsync(instanceId, showHistory: false, showHistoryOutput: false, showInput: false);

            return status is null ? (IActionResult)new NotFoundResult() : new OkObjectResult(status);
        }
    }
}
