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
    internal static class ProjectSetExtension
    {

        public static Task<Project> SetProjectAsync(this IDurableOrchestrationContext functionContext, Project project, bool allowUnsafe = false)
            => functionContext.IsLockedBy(project) || allowUnsafe
            ? functionContext.CallActivityWithRetryAsync<Project>(nameof(ProjectSetActivity), project)
            : throw new NotSupportedException($"Unable to set project '{project.Id}' without acquired lock");
    }
}
