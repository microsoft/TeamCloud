/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Orchestrations.Commands.Activities
{
    internal static class TeamCloudSetExtension
    {
        public static Task<TeamCloudInstance> SetTeamCloudAsync(this IDurableOrchestrationContext durableOrchestrationContext, TeamCloudInstance teamCloud)
        {
            if (teamCloud is null)
                throw new ArgumentNullException(nameof(teamCloud));

            if (durableOrchestrationContext.IsLocked(out var ownedLocks) && ownedLocks.Contains(teamCloud.GetEntityId()))
            {
                return durableOrchestrationContext
                    .CallActivityWithRetryAsync<TeamCloudInstance>(nameof(TeamCloudSetActivity), teamCloud);
            }

            throw new NotSupportedException($"Unable to set '{typeof(TeamCloudInstance)}' without acquired lock");
        }
    }
}
