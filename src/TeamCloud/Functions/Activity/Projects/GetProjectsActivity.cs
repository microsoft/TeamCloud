/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace TeamCloud
{
    public static class GetProjectsActivity
    {
        [FunctionName(nameof(GetProjectsActivity))]
        public static List<Project> RunActivity(
            [ActivityTrigger] TeamCloud teamCloud,
            [CosmosDB(nameof(TeamCloud), "Projects", PartitionKey = nameof(TeamCloud), ConnectionStringSetting = "AzureCosmosDBConnection")] IEnumerable<Project> projects)
        {
            return projects.ToList();
        }
    }
}
