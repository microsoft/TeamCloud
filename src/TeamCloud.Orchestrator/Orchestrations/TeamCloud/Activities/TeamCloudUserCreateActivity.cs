/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Orchestrations.TeamCloud.Activities
{
    public class TeamCloudUserCreateActivity
    {
        private readonly ITeamCloudRepository teamCloudRepository;

        public TeamCloudUserCreateActivity(ITeamCloudRepository teamCloudRepository)
        {
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
        }

        [FunctionName(nameof(TeamCloudUserCreateActivity))]
        public async Task<TeamCloudInstance> RunActivity(
            [ActivityTrigger] (TeamCloudInstance teamCloud, User newUser) input)
        {
            if (input.teamCloud is null)
                throw new ArgumentException($"input param must contain a valid TeamCloudInstance set on {nameof(input.teamCloud)}.", nameof(input));

            if (input.newUser is null)
                throw new ArgumentException($"input param must contain a valid User set on {nameof(input.newUser)}.", nameof(input));

            if (input.teamCloud.Users == null)
            {
                input.teamCloud.Users = new List<User>();
            }

            input.teamCloud.Users.Add(input.newUser);

            await teamCloudRepository
                .SetAsync(input.teamCloud)
                .ConfigureAwait(false);

            return input.teamCloud;
        }
    }
}
