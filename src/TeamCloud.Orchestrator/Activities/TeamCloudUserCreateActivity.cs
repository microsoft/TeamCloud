/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Activities
{
    public class TeamCloudUserCreateActivity
    {
        private readonly ITeamCloudRepository teamCloudRepository;

        public TeamCloudUserCreateActivity(ITeamCloudRepository teamCloudRepository)
        {
            this.teamCloudRepository = teamCloudRepository ?? throw new System.ArgumentNullException(nameof(teamCloudRepository));
        }

        [FunctionName(nameof(TeamCloudUserCreateActivity))]
        public Task<TeamCloudInstance> RunActivity(
            [ActivityTrigger] (TeamCloudInstance teamCloud, User newUser) input)
        {
            if (input.teamCloud.Users == null)
            {
                input.teamCloud.Users = new List<User>();
            }

            input.teamCloud.Users.Add(input.newUser);

            return Task.FromResult(input.teamCloud);
        }
    }
}
