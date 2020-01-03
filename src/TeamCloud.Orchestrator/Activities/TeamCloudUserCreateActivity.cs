/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model;

namespace TeamCloud.Orchestrator.Activities
{
    public static class TeamCloudUserCreateActivity
    {
        [FunctionName(nameof(TeamCloudUserCreateActivity))]
        public static TeamCloudInstance RunActivity(
            [ActivityTrigger] (TeamCloudInstance teamCloud, User newUser) input,
            [CosmosDB(Constants.CosmosDb.DatabaseName, nameof(TeamCloudInstance), Id = Constants.CosmosDb.TeamCloudInstanceId, PartitionKey = Constants.CosmosDb.TeamCloudInstanceId, ConnectionStringSetting = "AzureCosmosDBConnection")] TeamCloudInstance teamCloud)
        {
            if (teamCloud.Users == null)
            {
                teamCloud.Users = new List<User>();
            }

            teamCloud.Users.Add(input.newUser);

            return teamCloud;
        }
    }
}
