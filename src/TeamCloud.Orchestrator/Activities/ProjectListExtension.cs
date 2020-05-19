/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Activities
{
    internal static class ProjectListExtension
    {

        public static Task<IEnumerable<Project>> ListProjectsAsync(this IDurableOrchestrationContext durableOrchestrationContext)
            => durableOrchestrationContext.CallActivityWithRetryAsync<IEnumerable<Project>>(nameof(ProjectListActivity), null);

        public static Task<IEnumerable<Project>> ListProjectsAsync(this IDurableOrchestrationContext durableOrchestrationContext, IList<Guid> projectIds)
            => durableOrchestrationContext.CallActivityWithRetryAsync<IEnumerable<Project>>(nameof(ProjectListByIdActivity), projectIds);
    }
}
