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
        private readonly ITeamCloudRepository teamCloudRepository;
        private readonly IProjectsRepository projectsRepository;

        public ProjectDeleteActivity(ITeamCloudRepository teamCloudRepository, IProjectsRepository projectsRepository)
        {
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
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

            var oldProject = await projectsRepository
                .RemoveAsync(project)
                .ConfigureAwait(false);

            teamCloud.ProjectIds.Remove(oldProject.Id);

            await teamCloudRepository
                .SetAsync(teamCloud)
                .ConfigureAwait(false);

            return oldProject;
        }
    }
}
