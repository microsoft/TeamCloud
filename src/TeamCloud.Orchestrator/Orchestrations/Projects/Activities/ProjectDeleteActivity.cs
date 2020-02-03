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

namespace TeamCloud.Orchestrator.Orchestrations.Projects.Activities
{
    public class ProjectDeleteActivity
    {
        private readonly IProjectsRepository projectsRepository;
        private readonly ITeamCloudRepository teamCloudRepository;

        public ProjectDeleteActivity(ITeamCloudRepository teamCloudRepository, IProjectsRepository projectsRepository)
        {
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
        }

        [FunctionName(nameof(ProjectDeleteActivity))]
        public async Task<Project> RunActivity(
            [ActivityTrigger] Project project)
        {
            if (project is null)
                throw new ArgumentNullException(nameof(project));

            var teamCloud = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            _ = await projectsRepository
                .RemoveAsync(project)
                .ConfigureAwait(false);

            if (teamCloud.ProjectIds.Remove(project.Id))
            {
                await teamCloudRepository
                    .SetAsync(teamCloud)
                    .ConfigureAwait(false);
            }

            return project;
        }
    }
}
