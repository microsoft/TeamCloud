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
    internal static class TeamCloudGetExtension
    {

        public static Task<TeamCloudInstance> GetTeamCloudAsync(this IDurableOrchestrationContext durableOrchestrationContext)
            => durableOrchestrationContext.CallActivityWithRetryAsync<TeamCloudInstance>(nameof(TeamCloudGetActivity), null);
    }
}
