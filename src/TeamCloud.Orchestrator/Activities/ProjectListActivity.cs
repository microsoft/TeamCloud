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
        private readonly IProjectsRepository projectsRepository;

        public ProjectListActivity(IProjectsRepository projectsRepository)
        {
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
        }

        [FunctionName(nameof(ProjectListActivity))]
        public async Task<IEnumerable<ProjectDocument>> RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var projects = projectsRepository
                .ListAsync();

            return await projects
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }

    public class ProjectListByIdActivity
    {
        private readonly IProjectsRepository projectsRepository;

        public ProjectListByIdActivity(IProjectsRepository projectsRepository)
        {
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
        }

        [FunctionName(nameof(ProjectListByIdActivity))]
        public async Task<IEnumerable<ProjectDocument>> RunActivity(
            [ActivityTrigger] IList<string> projectIds)
        {
            var projects = projectsRepository
                .ListAsync(projectIds);

            return await projects
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }


    internal static class ProjectListExtension
    {
        public static Task<IEnumerable<ProjectDocument>> ListProjectsAsync(this IDurableOrchestrationContext durableOrchestrationContext)
            => durableOrchestrationContext.CallActivityWithRetryAsync<IEnumerable<ProjectDocument>>(nameof(ProjectListActivity), null);

        public static Task<IEnumerable<ProjectDocument>> ListProjectsAsync(this IDurableOrchestrationContext durableOrchestrationContext, IList<string> projectIds)
            => durableOrchestrationContext.CallActivityWithRetryAsync<IEnumerable<ProjectDocument>>(nameof(ProjectListByIdActivity), projectIds);
    }
}
