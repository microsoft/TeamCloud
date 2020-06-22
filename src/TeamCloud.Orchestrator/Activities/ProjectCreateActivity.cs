/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Internal.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Activities
{
    public class ProjectCreateActivity
    {
        private readonly IProjectsRepository projectsRepository;

        public ProjectCreateActivity(IProjectsRepository projectsRepository)
        {
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
        }

        [FunctionName(nameof(ProjectCreateActivity))]
        public async Task<Project> RunActivity(
            [ActivityTrigger] Project project)
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
        public static Task<Project> CreateProjectAsync(this IDurableOrchestrationContext functionContext, Project project)
            => functionContext.CallActivityWithRetryAsync<Project>(nameof(ProjectCreateActivity), project);
    }
}
