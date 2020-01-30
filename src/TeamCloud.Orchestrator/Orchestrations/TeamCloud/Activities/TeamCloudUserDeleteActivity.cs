/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Orchestrations.TeamCloud.Activities
{
    public class TeamCloudUserDeleteActivity
    {
        private readonly ITeamCloudRepository teamCloudRepository;

        public TeamCloudUserDeleteActivity(ITeamCloudRepository teamCloudRepository)
        {
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
        }

        [FunctionName(nameof(TeamCloudUserDeleteActivity))]
        public async Task<User> RunActivity(
            [ActivityTrigger] User user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var teamCloud = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloud.Users.Remove(user))
            {
                await teamCloudRepository
                    .SetAsync(teamCloud)
                    .ConfigureAwait(false);
            }

            return user;
        }
    }
}
