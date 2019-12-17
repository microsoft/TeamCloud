/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace TeamCloud
{
    public static class UpdateProjectActivity
    {
        [FunctionName(nameof(UpdateProjectActivity))]
        public static Project RunActivity(
            [ActivityTrigger] Project project,
            [CosmosDB(nameof(TeamCloud), "Projects", Id = "{project.id}", PartitionKey = nameof(TeamCloud), ConnectionStringSetting = "AzureCosmosDBConnection")] Project dbProject)
        {
            dbProject = project;

            return dbProject;
        }
    }
}
