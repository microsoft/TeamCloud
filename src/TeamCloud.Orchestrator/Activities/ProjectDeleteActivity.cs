/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model;

namespace TeamCloud.Orchestrator.Activities
{
    public static class ProjectDeleteActivity
    {
        [FunctionName(nameof(ProjectDeleteActivity))]
        public static async Task<TeamCloudInstance> RunActivity(
            [ActivityTrigger] Project project,
            [CosmosDB(nameof(TeamCloud), nameof(TeamCloud), Id = nameof(TeamCloud), PartitionKey = nameof(TeamCloud), ConnectionStringSetting = "AzureCosmosDBConnection")] TeamCloudInstance teamCloud,
            [CosmosDB(nameof(TeamCloud), "Projects", ConnectionStringSetting = "AzureCosmosDBConnection")] DocumentClient client)
        {
            var projectUri = UriFactory.CreateDocumentUri(nameof(TeamCloud), "Projects", project.Id);

            var deleteResponse = await client.DeleteDocumentAsync(projectUri);

            teamCloud.ProjectIds?.Remove(project.Id);

            return teamCloud;
        }
    }
}
