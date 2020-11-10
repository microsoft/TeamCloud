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
    public class ProjectDeleteActivity
    {
        private readonly IProjectRepository projectRepository;

        public ProjectDeleteActivity(IProjectRepository projectRepository)
        {
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        [FunctionName(nameof(ProjectDeleteActivity))]
        public async Task RunActivity(
            [ActivityTrigger] IDurableActivityContext activityContext)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            var project = activityContext.GetInput<Project>();

            _ = await projectRepository
                .RemoveAsync(project)
                .ConfigureAwait(false);
        }
    }

    internal static class ProjectDeleteExtension
    {
        public static Task<Project> DeleteProjectAsync(this IDurableOrchestrationContext orchestrationContext, Project project)
            => orchestrationContext.CallActivityWithRetryAsync<Project>(nameof(ProjectDeleteActivity), project);
    }
}
