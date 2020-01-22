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
    public class TeamCloudCreateActivity
    {
        private readonly ITeamCloudRepository teamCloudRepository;

        public TeamCloudCreateActivity(ITeamCloudRepository teamCloudRepository)
        {
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
        }

        [FunctionName(nameof(TeamCloudCreateActivity))]
        public async Task<TeamCloudInstance> RunActivity(
            [ActivityTrigger] TeamCloudInstance teamCloudInstance)
        {
            if (teamCloudInstance is null) throw new ArgumentNullException(nameof(teamCloudInstance));

            var teamCloud = await teamCloudRepository
                .SetAsync(teamCloudInstance)
                .ConfigureAwait(false);

            return teamCloud;
        }
    }
}