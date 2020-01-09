/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model;

namespace TeamCloud.Orchestrator.Activities
{
    public class ProjectUserDeleteActivity
    {
        private readonly IProjectsRepository projectsRepository;

        public ProjectUserDeleteActivity(IProjectsRepository projectsRepository)
        {
            this.projectsRepository = projectsRepository ?? throw new System.ArgumentNullException(nameof(projectsRepository));
        }

        [FunctionName(nameof(ProjectUserDeleteActivity))]
        public Task<Project> RunActivity(
            [ActivityTrigger] (Project project, User deleteUser) input)
        {
            var user = input.project.Users?.FirstOrDefault(u => u.Id == input.deleteUser.Id);

            if (user != null)
            {
                input.project.Users.Remove(user);
            }

            return Task.FromResult(input.project);
        }
    }
}
