/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Activities
{
    public class ProjectListActivity
    {
        private readonly IProjectRepository projectRepository;

        public ProjectListActivity(IProjectRepository projectRepository)
        {
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        [FunctionName(nameof(ProjectListActivity))]
        public async Task<IEnumerable<Project>> RunActivity(
            [ActivityTrigger] IDurableActivityContext activityContext)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            var organizationId = activityContext.GetInput<string>();

            var projects = projectRepository
                .ListAsync(organizationId);

            return await projects
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }

    public class ProjectListByIdActivity
    {
        private readonly IProjectRepository projectRepository;

        public ProjectListByIdActivity(IProjectRepository projectRepository)
        {
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        [FunctionName(nameof(ProjectListByIdActivity))]
        public async Task<IEnumerable<Project>> RunActivity(
            [ActivityTrigger] IDurableActivityContext activityContext)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            var input = activityContext.GetInput<Input>();

            var projects = projectRepository
                .ListAsync(input.Organization, input.ProjectIds);

            return await projects
                .ToListAsync()
                .ConfigureAwait(false);
        }

        internal struct Input
        {
            public string Organization { get; set; }

            public IList<string> ProjectIds { get; set; }
        }
    }


    internal static class ProjectListExtension
    {
        public static Task<IEnumerable<Project>> ListProjectsAsync(this IDurableOrchestrationContext durableOrchestrationContext, string organizationId)
            => durableOrchestrationContext.CallActivityWithRetryAsync<IEnumerable<Project>>(nameof(ProjectListActivity), organizationId);

        public static Task<IEnumerable<Project>> ListProjectsAsync(this IDurableOrchestrationContext durableOrchestrationContext, string organizationId, IList<string> projectIds)
            => durableOrchestrationContext.CallActivityWithRetryAsync<IEnumerable<Project>>(nameof(ProjectListByIdActivity), new ProjectListByIdActivity.Input { Organization = organizationId, ProjectIds = projectIds });
    }
}
