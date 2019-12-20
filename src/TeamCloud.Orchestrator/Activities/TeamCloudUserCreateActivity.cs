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
            [ActivityTrigger] (TeamCloudInstance teamCloud, TeamCloudUser newUser) input,
            [CosmosDB(nameof(TeamCloud), nameof(TeamCloud), Id = nameof(TeamCloud), PartitionKey = nameof(TeamCloud), ConnectionStringSetting = "AzureCosmosDBConnection")] TeamCloudInstance teamCloud)
        {
            if (teamCloud.Users == null)
                teamCloud.Users = new List<TeamCloudUser>();

            teamCloud.Users.Add(input.newUser);

            return teamCloud;
        }
    }
}
