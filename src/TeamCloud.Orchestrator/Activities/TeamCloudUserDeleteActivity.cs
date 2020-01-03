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
            [ActivityTrigger] (TeamCloudInstance teamCloud, User deleteUser) input,
            [CosmosDB(Constants.CosmosDb.DatabaseName, nameof(TeamCloudInstance), Id = Constants.CosmosDb.TeamCloudInstanceId, PartitionKey = Constants.CosmosDb.TeamCloudInstanceId, ConnectionStringSetting = "AzureCosmosDBConnection")] TeamCloudInstance teamCloud)
        {
            var user = teamCloud.Users?.FirstOrDefault(u => u.Id == input.deleteUser.Id);

            if (user != null)
            {
                teamCloud.Users.Remove(user);
            }

            return teamCloud;
        }
    }
}
