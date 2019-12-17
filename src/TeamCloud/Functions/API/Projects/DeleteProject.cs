/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace TeamCloud
{
    public static class DeleteProject
    {
        [FunctionName(nameof(DeleteProject))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "projects")] HttpRequest req,
            [CosmosDB(nameof(TeamCloud), nameof(TeamCloud), Id = nameof(TeamCloud), PartitionKey = nameof(TeamCloud), ConnectionStringSetting = "AzureCosmosDBConnection")] TeamCloud teamCloud,
            [CosmosDB(nameof(TeamCloud), "Projects", Id = "{projectId}", PartitionKey = nameof(TeamCloud), ConnectionStringSetting = "AzureCosmosDBConnection")] Project project,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var userAccess = req.ConfirmAccess(teamCloud, project, ProjectUserRole.Owner);
            if (!userAccess.HasAccess)
                return userAccess.UnauthorizedResult;

            // TODO: Update the body payload, this shouldn't be just a string

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var deleteId = JsonConvert.DeserializeObject<string>(requestBody);

            if (deleteId == null)
                return new BadRequestObjectResult("Please pass a project ID in the request body");

            if (project == null || string.IsNullOrEmpty(project.Id))
                return new NotFoundResult();

            var orchestratorContext = new OrchestratorContext(teamCloud, project, userAccess.User);

            string instanceId = await starter.StartNewAsync<object>(nameof(DeleteProjectUserOrchestration), orchestratorContext);

            log.LogInformation($"Started DeleteProjectOrchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
