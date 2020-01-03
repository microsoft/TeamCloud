/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model;

namespace TeamCloud.Orchestrator.Activities
{
    public static class ProjectCreateActivity
    {
        [FunctionName(nameof(ProjectCreateActivity))]
        public static Project RunActivity(
            [ActivityTrigger] Project newProject,
            [CosmosDB(Constants.CosmosDb.DatabaseName, nameof(TeamCloudInstance), Id = Constants.CosmosDb.TeamCloudInstanceId, PartitionKey = Constants.CosmosDb.TeamCloudInstanceId, ConnectionStringSetting = "AzureCosmosDBConnection")] TeamCloudInstance teamCloud,
            [CosmosDB(Constants.CosmosDb.DatabaseName, nameof(Project), PartitionKey = Constants.CosmosDb.TeamCloudInstanceId, ConnectionStringSetting = "AzureCosmosDBConnection")] out Project project)
        {
            project = newProject;

            teamCloud.ProjectIds.Add(project.Id);

            return project;
        }
    }
}
