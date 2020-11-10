/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Entities;

namespace TeamCloud.Orchestrator.Activities
{
    public class ProjectGetActivity
    {
        private readonly IProjectRepository projectRepository;

        public ProjectGetActivity(IProjectRepository projectRepository)
        {
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        [FunctionName(nameof(ProjectGetActivity))]
        public async Task<Project> RunActivity(
            [ActivityTrigger] IDurableActivityContext activityContext,
            ILogger log)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            var input = activityContext.GetInput<Input>();

            try
            {
                var project = await projectRepository
                    .GetAsync(input.Organization, input.Project)
                    .ConfigureAwait(false);

                return project;
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"Getting project {input.Project} from repository failed: {exc.Message}");

                throw;
            }
        }

        internal struct Input
        {
            public string Organization { get; set; }

            public string Project { get; set; }
        }
    }

    internal static class ProjectGetExtension
    {
        public static Task<Project> GetProjectAsync(this IDurableOrchestrationContext orchestrationContext, string organizationId, string projectId, bool allowUnsafe = false)
            => orchestrationContext.IsLockedBy<Project>(projectId) || allowUnsafe
            ? orchestrationContext.CallActivityWithRetryAsync<Project>(nameof(ProjectGetActivity), new ProjectGetActivity.Input { Organization = organizationId, Project = projectId })
            : throw new NotSupportedException($"Unable to get project '{projectId}' without acquired lock");
    }
}
