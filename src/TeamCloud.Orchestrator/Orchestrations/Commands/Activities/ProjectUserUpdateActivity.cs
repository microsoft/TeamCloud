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

namespace TeamCloud.Orchestrator.Orchestrations.Commands.Activities
{
    public class ProjectUserUpdateActivity
    {
        private readonly IProjectsRepository projectsRepository;

        public ProjectUserUpdateActivity(IProjectsRepository projectsRepository)
        {
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
        }

        [FunctionName(nameof(ProjectUserUpdateActivity))]
        public async Task<Project> RunActivity(
            [ActivityTrigger] (Guid projectId, User updateUser) input)
        {
            if (input.projectId == Guid.Empty)
                throw new ArgumentException($"input param must contain a valid ProjectID set on {nameof(input.projectId)}.", nameof(input));

            if (input.updateUser is null)
                throw new ArgumentException($"input param must contain a valid User set on {nameof(input.updateUser)}.", nameof(input));

            var project = await projectsRepository
                .GetAsync(input.projectId)
                .ConfigureAwait(false);

            var userIndex = project.Users.IndexOf(input.updateUser);

            if (userIndex >= 0)
            {
                project.Users[userIndex] = input.updateUser;

                await projectsRepository
                    .SetAsync(project)
                    .ConfigureAwait(false);
            }

            return project;
        }
    }
}
