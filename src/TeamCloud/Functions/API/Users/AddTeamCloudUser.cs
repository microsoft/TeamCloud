/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.IO;
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
    public static class AddTeamCloudUser
    {
        [FunctionName(nameof(AddTeamCloudUser))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users")] HttpRequest req,
            [CosmosDB(nameof(TeamCloud), nameof(TeamCloud), Id = nameof(TeamCloud), PartitionKey = nameof(TeamCloud), ConnectionStringSetting = "AzureCosmosDBConnection")] TeamCloud teamCloud,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var userAccess = req.ConfirmAccess(teamCloud, TeamCloudUserRole.Admin);
            if (!userAccess.HasAccess)
                return userAccess.UnauthorizedResult;

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var userDefinition = JsonConvert.DeserializeObject<TeamCloudUserDefinition>(requestBody);

            if (userDefinition == null)
                return new BadRequestObjectResult("Please pass a TeamCloudUserDefinition in the request body");

            // TODO: Check if the user already exists

            var orchestratorContext = new OrchestratorContext(teamCloud, null, userAccess.User);

            string instanceId = await starter.StartNewAsync<object>(nameof(AddProjectUserOrchestration), (orchestratorContext, userDefinition));

            log.LogInformation($"Started AddProjectUserOrchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
