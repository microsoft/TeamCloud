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
    public class ProjectCreateActivity
    {
        private readonly IProjectsRepository projectsRepository;

        public ProjectCreateActivity(IProjectsRepository projectsRepository)
        {
            this.projectsRepository = projectsRepository ?? throw new System.ArgumentNullException(nameof(projectsRepository));
        }

        [FunctionName(nameof(ProjectCreateActivity))]
        public async Task<Project> RunActivity(
            [ActivityTrigger] Project project)
        {
            if (project is null) throw new ArgumentNullException(nameof(project));

            var isExisting = await projectsRepository
                .NameExistsAsync(project)
                .ConfigureAwait(false);

            if (isExisting)
                throw new ArgumentException($"Project name '{project.Name}' already exists.");

            var newProject = await projectsRepository
                .AddAsync(project)
                .ConfigureAwait(false);

            return newProject;
        }
    }
}
