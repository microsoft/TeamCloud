/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Threading.Tasks;
using TeamCloud.Data;
using TeamCloud.Model;

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
            var project = await teamCloudRepository
                .SetAsync(teamCloudInstance)
                .ConfigureAwait(false);

            return project;
        }
    }
}