/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace TeamCloud
{
    public static class GetTeamCloudUsers
    {
        [FunctionName(nameof(GetTeamCloudUsers))]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users")] HttpRequest req,
            [CosmosDB(nameof(TeamCloud), nameof(TeamCloud), Id = nameof(TeamCloud), PartitionKey = nameof(TeamCloud), ConnectionStringSetting = "AzureCosmosDBConnection")] TeamCloud teamCloud)
        {
            var userAccess = req.ConfirmAccess(teamCloud, TeamCloudUserRole.Admin);
            if (!userAccess.HasAccess)
                return userAccess.UnauthorizedResult;

            return (ActionResult)new OkObjectResult(teamCloud.Users);
        }
    }
}
