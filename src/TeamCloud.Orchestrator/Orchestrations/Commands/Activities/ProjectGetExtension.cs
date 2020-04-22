/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Orchestrations.Utilities.Entities;

namespace TeamCloud.Orchestrator.Orchestrations.Commands.Activities
{
    internal static class ProjectGetExtension
    {

        public static Task<Project> GetProjectAsync(this IDurableOrchestrationContext functionContext, Guid projectId, bool allowUnsafe = false)
            => functionContext.IsLockedBy<Project>(projectId.ToString()) || allowUnsafe
            ? functionContext.CallActivityWithRetryAsync<Project>(nameof(ProjectGetActivity), projectId)
            : throw new NotSupportedException($"Unable to get project '{projectId}' without acquired lock");
    }
}
