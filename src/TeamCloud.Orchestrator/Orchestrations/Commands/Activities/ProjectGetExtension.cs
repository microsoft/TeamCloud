/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Orchestrations.Commands.Activities
{
    internal static class ProjectGetExtension
    {
        public static Task<Project> GetProjectAsync(this IDurableOrchestrationContext functionContext, Guid projectId, bool allowUnsafe = false)
        {
            if (functionContext.IsLockedBy<Project>(projectId.ToString()) || allowUnsafe)
            {
                return functionContext
                    .CallActivityWithRetryAsync<Project>(nameof(ProjectGetActivity), projectId);
            }

            throw new NotSupportedException($"Unable to get project '{projectId}' without acquired lock");
        }
    }
}
