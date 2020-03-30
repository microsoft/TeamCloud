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
    internal static class ProjectSetExtension
    {
        public static Task<Project> SetProjectAsync(this IDurableOrchestrationContext functionContext, Project project)
        {
            if (project is null)
                throw new System.ArgumentNullException(nameof(project));

            if (functionContext.IsLockedBy(project))
            {
                return functionContext
                    .CallActivityWithRetryAsync<Project>(nameof(ProjectSetActivity), project);
            }

            throw new NotSupportedException($"Unable to set '{project.GetType()}' without acquired lock");
        }

    }
}
