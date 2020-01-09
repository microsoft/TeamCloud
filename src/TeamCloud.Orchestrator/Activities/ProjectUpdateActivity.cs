/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Threading.Tasks;
using TeamCloud.Data;
using TeamCloud.Model;

namespace TeamCloud.Orchestrator.Activities
{
    public class ProjectUpdateActivity
    {
        private readonly IProjectsRepository projectsRepository;

        public ProjectUpdateActivity(IProjectsRepository projectsRepository)
        {
            this.projectsRepository = projectsRepository ?? throw new System.ArgumentNullException(nameof(projectsRepository));
        }

        [FunctionName(nameof(ProjectUpdateActivity))]
        public  Task<Project> RunActivity(
            [ActivityTrigger] Project project)
        {
            throw new NotImplementedException();
        }
    }
}
