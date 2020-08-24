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
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Activities
{
    public class ProjectCreateActivity
    {
        private readonly IProjectRepository projectsRepository;

        public ProjectCreateActivity(IProjectRepository projectsRepository)
        {
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
        }

        [FunctionName(nameof(ProjectCreateActivity))]
        public async Task<ProjectDocument> RunActivity(
            [ActivityTrigger] ProjectDocument project)
        {
            if (project is null)
                throw new ArgumentNullException(nameof(project));

            project = await projectsRepository
                .AddAsync(project)
                .ConfigureAwait(false);

            return project;
        }
    }

    internal static class ProjectCreateExtension
    {
        public static Task<ProjectDocument> CreateProjectAsync(this IDurableOrchestrationContext functionContext, ProjectDocument project)
            => functionContext.CallActivityWithRetryAsync<ProjectDocument>(nameof(ProjectCreateActivity), project);
    }
}
