/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model;

namespace TeamCloud.Orchestrator.Activities
{
    public static class TeamCloudUserDeleteActivity
    {
        [FunctionName(nameof(TeamCloudUserDeleteActivity))]
        public static TeamCloudInstance RunActivity(
            [ActivityTrigger] (TeamCloudInstance teamCloud, TeamCloudUser deleteUser) input,
            [CosmosDB(nameof(TeamCloud), nameof(TeamCloud), Id = nameof(TeamCloud), PartitionKey = nameof(TeamCloud), ConnectionStringSetting = "AzureCosmosDBConnection")] TeamCloudInstance teamCloud)
        {
            var user = teamCloud.Users?.FirstOrDefault(u => u.Id == input.deleteUser.Id);

            if (user != null)
                teamCloud.Users.Remove(user);

            return teamCloud;
        }
    }
}
