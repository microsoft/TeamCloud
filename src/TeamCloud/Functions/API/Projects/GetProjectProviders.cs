/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TeamCloud
{
    public static class GetProjectProviders
    {
        [FunctionName(nameof(GetProjectProviders))]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "projects/{projectId}/providers")] HttpRequest req,
            [CosmosDB(nameof(TeamCloud), nameof(TeamCloud), Id = nameof(TeamCloud), PartitionKey = nameof(TeamCloud), ConnectionStringSetting = "AzureCosmosDBConnection")] TeamCloud teamCloud,
            [CosmosDB(nameof(TeamCloud), "Projects", Id = "{projectId}", PartitionKey = nameof(TeamCloud), ConnectionStringSetting = "AzureCosmosDBConnection")] Project project,
            ILogger log)
        {
            var userAccess = req.ConfirmAccess(teamCloud, project, ProjectUserRole.Member);
            if (!userAccess.HasAccess)
                return userAccess.UnauthorizedResult;

            // TODO: return project providers once implemented

            return (ActionResult)new OkObjectResult(project);
        }
    }
}
