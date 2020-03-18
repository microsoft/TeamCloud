/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Orchestrations.Commands.Activities
{
    internal static class ProjectSetExtension
    {
        public static Task<Project> SetProjectAsync(this IDurableOrchestrationContext durableOrchestrationContext, Project project)
            => durableOrchestrationContext.CallActivityWithRetryAsync<Project>(nameof(ProjectSetActivity), project);
    }
}
