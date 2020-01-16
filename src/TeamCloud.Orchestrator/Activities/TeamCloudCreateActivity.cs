/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Activities
{
    public class TeamCloudCreateActivity
    {
        private readonly ITeamCloudRepository teamCloudRepository;

        public TeamCloudCreateActivity(ITeamCloudRepository teamCloudRepository)
        {
            this.teamCloudRepository = teamCloudRepository ?? throw new System.ArgumentNullException(nameof(teamCloudRepository));
        }

        [FunctionName(nameof(TeamCloudCreateActivity))]
        public async Task<TeamCloudInstance> RunActivity(
            [ActivityTrigger] TeamCloudInstance teamCloudInstance)
        {
            var teamCloud = await teamCloudRepository
                .SetAsync(teamCloudInstance)
                .ConfigureAwait(false);

            return teamCloud;
        }
    }
}