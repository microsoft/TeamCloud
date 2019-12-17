/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace TeamCloud
{
    public static class AddTeamCloudUserActivity
    {
        [FunctionName(nameof(AddTeamCloudUserActivity))]
        public static TeamCloud RunActivity(
            [ActivityTrigger] (TeamCloud teamCloud, TeamCloudUser newUser) input,
            [CosmosDB(nameof(TeamCloud), nameof(TeamCloud), Id = nameof(TeamCloud), PartitionKey = nameof(TeamCloud), ConnectionStringSetting = "AzureCosmosDBConnection")] TeamCloud teamCloud)
        {
            if (teamCloud.Users == null)
                teamCloud.Users = new List<TeamCloudUser>();

            teamCloud.Users.Add(input.newUser);

            return teamCloud;
        }
    }
}
