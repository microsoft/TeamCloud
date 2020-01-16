/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Activities
{
    public class TeamCloudUserDeleteActivity
    {
        private readonly ITeamCloudRepository teamCloudRepository;

        public TeamCloudUserDeleteActivity(ITeamCloudRepository teamCloudRepository)
        {
            this.teamCloudRepository = teamCloudRepository ?? throw new System.ArgumentNullException(nameof(teamCloudRepository));
        }

        [FunctionName(nameof(TeamCloudUserDeleteActivity))]
        public Task<TeamCloudInstance> RunActivity(
            [ActivityTrigger] (TeamCloudInstance teamCloud, User deleteUser) input)
        {
            var user = input.teamCloud.Users?.FirstOrDefault(u => u.Id == input.deleteUser.Id);

            if (user != null)
            {
                input.teamCloud.Users.Remove(user);
            }

            return Task.FromResult(input.teamCloud);
        }
    }
}
