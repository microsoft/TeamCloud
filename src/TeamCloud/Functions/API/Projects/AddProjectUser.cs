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
    public static class AddProjectUser
    {
        [FunctionName(nameof(AddProjectUser))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "projects/{projectId}/users")] HttpRequest req,
            [CosmosDB(nameof(TeamCloud), nameof(TeamCloud), Id = nameof(TeamCloud), PartitionKey = nameof(TeamCloud), ConnectionStringSetting = "AzureCosmosDBConnection")] TeamCloud teamCloud,
            [CosmosDB(nameof(TeamCloud), "Projects", Id = "{projectId}", PartitionKey = nameof(TeamCloud), ConnectionStringSetting = "AzureCosmosDBConnection")] Project project,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var userAccess = req.ConfirmAccess(teamCloud, project, ProjectUserRole.Owner);
            if (!userAccess.HasAccess)
                return userAccess.UnauthorizedResult;

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var userDefinition = JsonConvert.DeserializeObject<ProjectUserDefinition>(requestBody);

            if (userDefinition == null)
                return new BadRequestObjectResult("Please pass a ProjectUserDefinition in the request body");

            // TODO: Check if the user already exists?

            var orchestratorContext = new OrchestratorContext(teamCloud, project, userAccess.User);

            string instanceId = await starter.StartNewAsync<object>(nameof(AddProjectUserOrchestration), (orchestratorContext, userDefinition));

            log.LogInformation($"Started AddProjectUserOrchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
