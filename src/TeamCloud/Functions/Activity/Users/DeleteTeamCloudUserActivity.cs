/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace TeamCloud
{
    public static class DeleteTeamCloudUserActivity
    {
        [FunctionName(nameof(DeleteTeamCloudUserActivity))]
        public static TeamCloud RunActivity(
            [ActivityTrigger] (TeamCloud teamCloud, TeamCloudUser deleteUser) input,
            [CosmosDB(nameof(TeamCloud), nameof(TeamCloud), Id = nameof(TeamCloud), PartitionKey = nameof(TeamCloud), ConnectionStringSetting = "AzureCosmosDBConnection")] TeamCloud teamCloud)
        {
            var user = teamCloud.Users?.FirstOrDefault(u => u.Id == input.deleteUser.Id);

            if (user != null)
                teamCloud.Users.Remove(user);

            return teamCloud;
        }
    }
}
