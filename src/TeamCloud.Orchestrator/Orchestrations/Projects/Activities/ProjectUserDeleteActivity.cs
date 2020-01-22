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
        public async Task<Project> RunActivity(
            [ActivityTrigger] (Project project, User deleteUser) input)
        {
            if (input.project is null)
                throw new ArgumentException($"input param must contain a valid Project set on {nameof(input.project)}.", nameof(input));

            if (input.deleteUser is null)
                throw new ArgumentException($"input param must contain a valid User set on {nameof(input.deleteUser)}.", nameof(input));

            var user = input.project.Users?.FirstOrDefault(u => u.Id == input.deleteUser.Id);

            if (user != null)
            {
                input.project.Users.Remove(user);
            }

            await projectsRepository
                .SetAsync(input.project)
                .ConfigureAwait(false);

            return input.project;
        }
    }
}
