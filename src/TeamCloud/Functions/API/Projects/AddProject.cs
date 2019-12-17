/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TeamCloud
{
    public static class AddProject
    {
        [FunctionName(nameof(AddProject))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "projects")] HttpRequest req,
            [CosmosDB(nameof(TeamCloud), nameof(TeamCloud), Id = nameof(TeamCloud), PartitionKey = nameof(TeamCloud), ConnectionStringSetting = "AzureCosmosDBConnection")] TeamCloud teamCloud,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var userAccess = req.ConfirmAccess(teamCloud, TeamCloudUserRole.Creator);
            if (!userAccess.HasAccess)
                return userAccess.UnauthorizedResult;

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var projectDefinition = JsonConvert.DeserializeObject<ProjectDefinition>(requestBody);

            if (projectDefinition == null)
                return new BadRequestObjectResult("Please pass a ProjectDefinition in the request body");

            var orchestratorContext = new OrchestratorContext(teamCloud, null, userAccess.User);

            string instanceId = await starter.StartNewAsync<object>(nameof(AddProjectOrchestration), (orchestratorContext, projectDefinition));

            log.LogInformation($"Started AddProjectOrchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
