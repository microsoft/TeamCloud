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

            var projects = projectRepository
                .ListAsync();

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
            [ActivityTrigger] IList<string> projectIds)
        {
            var projects = projectRepository
                .ListAsync(projectIds);

            return await projects
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }


    internal static class ProjectListExtension
    {
        public static Task<IEnumerable<Project>> ListProjectsAsync(this IDurableOrchestrationContext durableOrchestrationContext)
            => durableOrchestrationContext.CallActivityWithRetryAsync<IEnumerable<Project>>(nameof(ProjectListActivity), null);

        public static Task<IEnumerable<Project>> ListProjectsAsync(this IDurableOrchestrationContext durableOrchestrationContext, IList<string> projectIds)
            => durableOrchestrationContext.CallActivityWithRetryAsync<IEnumerable<Project>>(nameof(ProjectListByIdActivity), projectIds);
    }
}
