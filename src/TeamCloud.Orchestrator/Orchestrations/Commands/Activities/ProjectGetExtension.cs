using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Orchestrations.Commands.Activities
{
    internal static class ProjectGetExtension
    {
        public static Task<Project> GetProjectAsync(this IDurableOrchestrationContext durableOrchestrationContext, Guid projectId)
            => durableOrchestrationContext.CallActivityWithRetryAsync<Project>(nameof(ProjectGetActivity), projectId);
    }
}
