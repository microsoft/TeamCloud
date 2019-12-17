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
    public static class GetProjects
    {
        [FunctionName(nameof(GetProjects))]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "projects")] HttpRequest req,
            [CosmosDB(nameof(TeamCloud), nameof(TeamCloud), Id = nameof(TeamCloud), PartitionKey = nameof(TeamCloud), ConnectionStringSetting = "AzureCosmosDBConnection")] TeamCloud teamCloud)
        {
            var userAccess = req.ConfirmAccess(teamCloud, TeamCloudUserRole.Admin);
            if (!userAccess.HasAccess)
                return userAccess.UnauthorizedResult;

            return (ActionResult)new OkObjectResult(teamCloud.ProjectIds);
        }
    }
}
