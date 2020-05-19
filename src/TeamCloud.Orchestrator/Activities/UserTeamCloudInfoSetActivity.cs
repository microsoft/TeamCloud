/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Activities
{
    public class UserTeamCloudInfoSetActivity
    {
        private readonly IUsersRepository usersRepository;

        public UserTeamCloudInfoSetActivity(IUsersRepository usersRepository)
        {
            this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
        }

        [FunctionName(nameof(UserTeamCloudInfoSetActivity))]
        public async Task<User> RunActivity(
            [ActivityTrigger] User user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var newUser = await usersRepository
                .SetTeamCloudInfoAsync(user)
                .ConfigureAwait(false);

            return newUser;
        }
    }
}
