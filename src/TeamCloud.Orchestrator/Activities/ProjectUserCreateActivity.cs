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

namespace TeamCloud.Orchestrator.Activities
{
    public class ProjectUserCreateActivity
    {
        private readonly IProjectsRepository projectsRepository;

        public ProjectUserCreateActivity(IProjectsRepository projectsRepository)
        {
            this.projectsRepository = projectsRepository ?? throw new System.ArgumentNullException(nameof(projectsRepository));
        }

        [FunctionName(nameof(ProjectUserCreateActivity))]
        public async Task<Project> RunActivity(
            [ActivityTrigger] (Project project, User newUser) input)
        {
            if (input.project is null)
                throw new ArgumentException($"input param must contain a valid Project set on {nameof(input.project)}.", nameof(input));

            if (input.newUser is null)
                throw new ArgumentException($"input param must contain a valid User set on {nameof(input.newUser)}.", nameof(input));

            if (input.project.Users == null)
            {
                input.project.Users = new List<User>();
            }

            input.project.Users.Add(input.newUser);

            await projectsRepository
                .SetAsync(input.project)
                .ConfigureAwait(false);

            return input.project;
        }
    }
}
