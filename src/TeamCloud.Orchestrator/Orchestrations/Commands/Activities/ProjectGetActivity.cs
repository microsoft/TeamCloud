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
    public class ProjectGetActivity
    {
        private readonly IProjectsRepository projectsRepository;

        public ProjectGetActivity(IProjectsRepository projectsRepository)
        {
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
        }

        [FunctionName(nameof(ProjectGetActivity))]
        public async Task<Project> RunActivity(
            [ActivityTrigger] Guid projectId)
        {
            return await projectsRepository
                .GetAsync(projectId)
                .ConfigureAwait(false);
        }
    }
}
