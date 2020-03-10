using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Orchestrations.TeamCloud.Activities
{
    internal static class TeamCloudGetExtension
    {
        public static Task<TeamCloudInstance> GetTeamCloudAsync(this IDurableOrchestrationContext durableOrchestrationContext)
        {
            return durableOrchestrationContext
                .CallActivityWithRetryAsync<TeamCloudInstance>(nameof(TeamCloudGetActivity), null);
        }
    }
}
