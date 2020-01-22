/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

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

        public ProjectDeleteActivity(IProjectsRepository projectsRepository)
        {
            this.projectsRepository = projectsRepository ?? throw new System.ArgumentNullException(nameof(projectsRepository));
        }

        [FunctionName(nameof(ProjectDeleteActivity))]
        public async Task<Project> RunActivity(
            [ActivityTrigger] Project project)
        {
            return await projectsRepository
                .RemoveAsync(project)
                .ConfigureAwait(false);
        }
    }
}
