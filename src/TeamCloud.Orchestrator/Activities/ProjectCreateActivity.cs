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
    public class ProjectCreateActivity
    {
        private readonly IProjectsRepository projectsRepository;

        public ProjectCreateActivity(IProjectsRepository projectsRepository)
        {
            this.projectsRepository = projectsRepository ?? throw new System.ArgumentNullException(nameof(projectsRepository));
        }

        [FunctionName(nameof(ProjectCreateActivity))]
        public async Task<Project> RunActivity(
            [ActivityTrigger] Project newProject)
        {
            var project = await projectsRepository
                .AddAsync(newProject)
                .ConfigureAwait(false);

            return project;
        }
    }
}
