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
        private readonly IProjectRepository projectRepository;

        public ProjectCreateActivity(IProjectRepository projectRepository)
        {
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        [FunctionName(nameof(ProjectCreateActivity))]
        public async Task<Project> RunActivity(
            [ActivityTrigger] IDurableActivityContext activityContext)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            var project = activityContext.GetInput<Project>();

            project = await projectRepository
                .AddAsync(project)
                .ConfigureAwait(false);

            return project;
        }
    }

    internal static class ProjectCreateExtension
    {
        public static Task<Project> CreateProjectAsync(this IDurableOrchestrationContext orchestrationContext, Project project)
            => orchestrationContext.CallActivityWithRetryAsync<Project>(nameof(ProjectCreateActivity), project);
    }
}
