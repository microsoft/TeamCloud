/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Orchestrations.Commands.Activities
{
    public class ProjectUserCreateActivity
    {
        private readonly IProjectsRepository projectsRepository;

        public ProjectUserCreateActivity(IProjectsRepository projectsRepository)
        {
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
        }

        [FunctionName(nameof(ProjectUserCreateActivity))]
        public async Task<User> RunActivity(
            [ActivityTrigger] (Guid projectId, User newUser) input)
        {
            if (input.projectId == Guid.Empty)
                throw new ArgumentException($"input param must contain a valid ProjectID set on {nameof(input.projectId)}.", nameof(input));

            if (input.newUser is null)
                throw new ArgumentException($"input param must contain a valid User set on {nameof(input.newUser)}.", nameof(input));

            var project = await projectsRepository
                .GetAsync(input.projectId)
                .ConfigureAwait(false);

            project.Users.Add(input.newUser);

            await projectsRepository
                .SetAsync(project)
                .ConfigureAwait(false);

            return input.newUser;
        }
    }
}
