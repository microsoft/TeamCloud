/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Internal.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Entities;

namespace TeamCloud.Orchestrator.Activities
{
    public class TeamCloudSetActivity
    {
        private readonly ITeamCloudRepository teamCloudRepository;

        public TeamCloudSetActivity(ITeamCloudRepository teamCloudRepository)
        {
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
        }

        [FunctionName(nameof(TeamCloudSetActivity))]
        public async Task<TeamCloudInstanceDocument> RunActivity(
            [ActivityTrigger] TeamCloudInstanceDocument teamCloudInstance)
        {
            if (teamCloudInstance is null)
                throw new ArgumentNullException(nameof(teamCloudInstance));

            teamCloudInstance = await teamCloudRepository
                .SetAsync(teamCloudInstance)
                .ConfigureAwait(false);

            return teamCloudInstance;
        }
    }

    internal static class TeamCloudSetExtension
    {
        public static Task<TeamCloudInstanceDocument> SetTeamCloudAsync(this IDurableOrchestrationContext functionContext, TeamCloudInstanceDocument teamCloud)
        {
            if (teamCloud is null)
                throw new ArgumentNullException(nameof(teamCloud));

            if (functionContext.IsLockedByContainerDocument(teamCloud))
            {
                return functionContext
                    .CallActivityWithRetryAsync<TeamCloudInstanceDocument>(nameof(TeamCloudSetActivity), teamCloud);
            }

            throw new NotSupportedException($"Unable to set '{typeof(TeamCloudInstanceDocument)}' without acquired lock");
        }
    }
}
