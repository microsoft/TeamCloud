/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using Microsoft.Azure.Documents;
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
            [CosmosDB(Constants.CosmosDb.DatabaseName, nameof(TeamCloudInstance), Id = Constants.CosmosDb.TeamCloudInstanceId, PartitionKey = Constants.CosmosDb.TeamCloudInstanceId, ConnectionStringSetting = "AzureCosmosDBConnection")] TeamCloudInstance teamCloud,
            [CosmosDB(Constants.CosmosDb.DatabaseName, nameof(Project), ConnectionStringSetting = "AzureCosmosDBConnection")] DocumentClient client)
        {
            var projectUri = UriFactory.CreateDocumentUri(Constants.CosmosDb.DatabaseName, nameof(Project), project.Id.ToString());

            var deleteResponse = await client.DeleteDocumentAsync(projectUri, new RequestOptions { PartitionKey = new PartitionKey(Constants.CosmosDb.TeamCloudInstanceId) });

            teamCloud.ProjectIds?.Remove(project.Id);

            return teamCloud;
        }
    }
}
