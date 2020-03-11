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

namespace TeamCloud.Orchestrator.Orchestrations.Commands.Activities
{
    public class TeamCloudCreateActivity
    {
        private readonly ITeamCloudRepository teamCloudRepository;
        private readonly IProjectTypesRepository projectTypesRepository;

        public TeamCloudCreateActivity(ITeamCloudRepository teamCloudRepository, IProjectTypesRepository projectTypesRepository)
        {
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
            this.projectTypesRepository = projectTypesRepository ?? throw new ArgumentNullException(nameof(projectTypesRepository));
        }

        [FunctionName(nameof(TeamCloudCreateActivity))]
        public async Task<TeamCloudInstance> RunActivity(
            [ActivityTrigger] TeamCloudConfiguration teamCloudConfiguration)
        {
            if (teamCloudConfiguration is null)
                throw new ArgumentNullException(nameof(teamCloudConfiguration));

            var teamCloudInstance = new TeamCloudInstance(teamCloudConfiguration);

            var teamCloud = await teamCloudRepository
                .SetAsync(teamCloudInstance)
                .ConfigureAwait(false);

            foreach (var projectType in teamCloudConfiguration.ProjectTypes)
            {
                await projectTypesRepository
                    .AddAsync(projectType)
                    .ConfigureAwait(false);
            }

            return teamCloud;
        }
    }
}
