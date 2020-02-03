/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Orchestrations.Projects.Activities
{
    public class ProjectUserDeleteActivity
    {
        private readonly IProjectsRepository projectsRepository;

        public ProjectUserDeleteActivity(IProjectsRepository projectsRepository)
        {
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
        }

        [FunctionName(nameof(ProjectUserDeleteActivity))]
        public async Task<User> RunActivity(
            [ActivityTrigger] (Guid projectId, User deleteUser) input)
        {
            if (input.projectId == Guid.Empty)
                throw new ArgumentException($"input param must contain a valid ProjectID set on {nameof(input.projectId)}.", nameof(input));

            if (input.deleteUser is null)
                throw new ArgumentException($"input param must contain a valid User set on {nameof(input.deleteUser)}.", nameof(input));

            var project = await projectsRepository
                .GetAsync(input.projectId)
                .ConfigureAwait(false);

            if (project.Users.Remove(input.deleteUser))
            {
                await projectsRepository
                    .SetAsync(project)
                    .ConfigureAwait(false);
            }

            return input.deleteUser;
        }
    }
}
