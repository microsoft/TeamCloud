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
    public static class GetProjectUsers
    {
        [FunctionName(nameof(GetProjectUsers))]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "projects/{projectId}/users")] HttpRequest req,
            [CosmosDB(nameof(TeamCloud), nameof(TeamCloud), Id = nameof(TeamCloud), PartitionKey = nameof(TeamCloud), ConnectionStringSetting = "AzureCosmosDBConnection")] TeamCloud teamCloud,
            [CosmosDB(nameof(TeamCloud), "Projects", Id = "{projectId}", PartitionKey = nameof(TeamCloud), ConnectionStringSetting = "AzureCosmosDBConnection")] Project project)
        {
            var userAccess = req.ConfirmAccess(teamCloud, project, ProjectUserRole.Member);
            if (!userAccess.HasAccess)
                return userAccess.UnauthorizedResult;
                
            return (ActionResult)new OkObjectResult(project.Users);
        }
    }
}
