/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

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
        public Task<Project> RunActivity(
            [ActivityTrigger] (Project project, User newUser) input)
        {
            if (input.project.Users == null)
            {
                input.project.Users = new List<User>();
            }

            input.project.Users.Add(input.newUser);

            return Task.FromResult(input.project);
        }
    }
}
