/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace TeamCloud
{
    public static class DeleteTeamCloudUser
    {
        [FunctionName(nameof(DeleteTeamCloudUser))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "users")] HttpRequest req,
            [CosmosDB(nameof(TeamCloud), nameof(TeamCloud), Id = nameof(TeamCloud), PartitionKey = nameof(TeamCloud), ConnectionStringSetting = "AzureCosmosDBConnection")] TeamCloud teamCloud,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var userAccess = req.ConfirmAccess(teamCloud, TeamCloudUserRole.Admin);
            if (!userAccess.HasAccess)
                return userAccess.UnauthorizedResult;

            // TODO: Update the body payload, this shouldn't be just a string

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var deleteId = JsonConvert.DeserializeObject<string>(requestBody);

            if (deleteId == null)
                return new BadRequestObjectResult("Please pass a user ID in the request body");

            var deleteUser = teamCloud.Users?.FirstOrDefault(u => u.Id == deleteId);

            if (deleteUser == null)
                return new NotFoundResult();

            var orchestratorContext = new OrchestratorContext(teamCloud, null, userAccess.User);

            string instanceId = await starter.StartNewAsync<object>(nameof(DeleteTeamCloudUserOrchestration), (orchestratorContext, deleteId));

            log.LogInformation($"Started DeleteTeamCloudUserOrchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
