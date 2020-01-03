/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model;

namespace TeamCloud.Orchestrator.Activities
{
    public static class ProjectGetActivity
    {
        [FunctionName(nameof(ProjectGetActivity))]
        public static List<Project> RunActivity(
            [ActivityTrigger] TeamCloudInstance teamCloud,
            [CosmosDB(Constants.CosmosDb.DatabaseName, nameof(Project), PartitionKey = Constants.CosmosDb.TeamCloudInstanceId, ConnectionStringSetting = "AzureCosmosDBConnection")] IEnumerable<Project> projects)
        {
            return projects.ToList();
        }
    }
}
