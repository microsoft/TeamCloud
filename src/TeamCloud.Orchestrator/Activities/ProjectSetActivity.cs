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
using TeamCloud.Orchestrator.Entities;

namespace TeamCloud.Orchestrator.Activities
{
    public class ProjectSetActivity
    {
        private readonly IProjectsRepository projectsRepository;

        public ProjectSetActivity(IProjectsRepository projectsRepository)
        {
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
        }

        [FunctionName(nameof(ProjectSetActivity))]
        public async Task<ProjectDocument> RunActivity(
            [ActivityTrigger] ProjectDocument project)
        {
            if (project is null)
                throw new ArgumentNullException(nameof(project));

            var newProject = await projectsRepository
                .SetAsync(project)
                .ConfigureAwait(false);

            return newProject;
        }
    }

    internal static class ProjectSetExtension
    {
        public static Task<ProjectDocument> SetProjectAsync(this IDurableOrchestrationContext functionContext, ProjectDocument project, bool allowUnsafe = false)
            => functionContext.IsLockedByContainerDocument(project) || allowUnsafe
            ? functionContext.CallActivityWithRetryAsync<ProjectDocument>(nameof(ProjectSetActivity), project)
            : throw new NotSupportedException($"Unable to set project '{project.Id}' without acquired lock");
    }
}
