/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Activities
{
    internal static class ProjectDeleteExtension
    {
        public static Task<ProjectDocument> DeleteProjectAsync(this IDurableOrchestrationContext functionContext, ProjectDocument project)
            => functionContext.CallActivityWithRetryAsync<ProjectDocument>(nameof(ProjectDeleteActivity), project);
    }
}
