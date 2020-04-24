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
    internal static class TeamCloudSetExtension
    {
        public static Task<TeamCloudInstance> SetTeamCloudAsync(this IDurableOrchestrationContext functionContext, TeamCloudInstance teamCloud)
        {
            if (teamCloud is null)
                throw new ArgumentNullException(nameof(teamCloud));

            if (functionContext.IsLockedBy(teamCloud))
            {
                return functionContext
                    .CallActivityWithRetryAsync<TeamCloudInstance>(nameof(TeamCloudSetActivity), teamCloud);
            }

            throw new NotSupportedException($"Unable to set '{typeof(TeamCloudInstance)}' without acquired lock");
        }
    }
}
