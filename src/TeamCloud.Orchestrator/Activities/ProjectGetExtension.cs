/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Entities;

namespace TeamCloud.Orchestrator.Activities
{
    internal static class ProjectGetExtension
    {

        public static Task<Project> GetProjectAsync(this IDurableOrchestrationContext functionContext, Guid projectId, bool allowDirtyRead = false)
            => functionContext.IsLockedBy<Project>(projectId.ToString()) || allowDirtyRead
            ? functionContext.CallActivityWithRetryAsync<Project>(nameof(ProjectGetActivity), projectId)
            : throw new NotSupportedException($"Unable to get project '{projectId}' without acquired lock");
    }
}
