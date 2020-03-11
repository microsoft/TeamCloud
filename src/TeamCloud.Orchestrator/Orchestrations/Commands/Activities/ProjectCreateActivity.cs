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
    public class ProjectCreateActivity
    {
        private readonly ITeamCloudRepository teamCloudRepository;
        private readonly IProjectsRepository projectsRepository;

        public ProjectCreateActivity(ITeamCloudRepository teamCloudRepository, IProjectsRepository projectsRepository)
        {
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
        }

        [FunctionName(nameof(ProjectCreateActivity))]
        public async Task<Project> RunActivity(
            [ActivityTrigger] Project project)
        {
            if (project is null)
                throw new ArgumentNullException(nameof(project));

            var newProject = await projectsRepository
                .AddAsync(project)
                .ConfigureAwait(false);

            return newProject;
        }
    }
}
