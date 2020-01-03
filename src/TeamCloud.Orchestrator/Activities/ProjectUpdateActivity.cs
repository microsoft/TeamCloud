/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model;

namespace TeamCloud.Orchestrator.Activities
{
    public static class ProjectUpdateActivity
    {
        [FunctionName(nameof(ProjectUpdateActivity))]
        public static Project RunActivity(
            [ActivityTrigger] Project project,
            [CosmosDB(Constants.CosmosDb.DatabaseName, nameof(Project), Id = "{project.id}", PartitionKey = Constants.CosmosDb.TeamCloudInstanceId, ConnectionStringSetting = "AzureCosmosDBConnection")] Project dbProject)
        {
            dbProject = project;

            return dbProject;
        }
    }
}
