/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Activities
{
    public class TeamCloudUserDeleteActivity
    {
        private readonly ITeamCloudRepository teamCloudRepository;

        public TeamCloudUserDeleteActivity(ITeamCloudRepository teamCloudRepository)
        {
            this.teamCloudRepository = teamCloudRepository ?? throw new System.ArgumentNullException(nameof(teamCloudRepository));
        }

        [FunctionName(nameof(TeamCloudUserDeleteActivity))]
        public async Task<TeamCloudInstance> RunActivity(
            [ActivityTrigger] (TeamCloudInstance teamCloud, User deleteUser) input)
        {
            if (input.teamCloud is null)
                throw new ArgumentException($"input param must contain a valid TeamCloudInstance set on {nameof(input.teamCloud)}.", nameof(input));

            if (input.deleteUser is null)
                throw new ArgumentException($"input param must contain a valid User set on {nameof(input.deleteUser)}.", nameof(input));

            var user = input.teamCloud.Users?.FirstOrDefault(u => u.Id == input.deleteUser.Id);

            if (user != null)
            {
                input.teamCloud.Users.Remove(user);
            }

            await teamCloudRepository
                .SetAsync(input.teamCloud)
                .ConfigureAwait(false);

            return input.teamCloud;
        }
    }
}
