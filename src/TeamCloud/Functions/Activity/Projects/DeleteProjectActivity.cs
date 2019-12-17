/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace TeamCloud
{
    public static class DeleteProjectActivity
    {
        [FunctionName(nameof(DeleteProjectActivity))]
        public static async Task<TeamCloud> RunActivity(
            [ActivityTrigger] Project project,
            [CosmosDB(nameof(TeamCloud), nameof(TeamCloud), Id = nameof(TeamCloud), PartitionKey = nameof(TeamCloud), ConnectionStringSetting = "AzureCosmosDBConnection")] TeamCloud teamCloud,
            [CosmosDB(nameof(TeamCloud), "Projects", ConnectionStringSetting = "AzureCosmosDBConnection")] DocumentClient client)
        {
            var projectUri = UriFactory.CreateDocumentUri(nameof(TeamCloud), "Projects", project.Id);

            var deleteResponse = await client.DeleteDocumentAsync(projectUri);

            teamCloud.ProjectIds?.Remove(project.Id);

            return teamCloud;
        }
    }
}
