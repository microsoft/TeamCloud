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
    public class UserGetActivity
    {
        private readonly IUsersRepository usersRepository;

        public UserGetActivity(IUsersRepository usersRepository)
        {
            this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
        }

        [FunctionName(nameof(UserGetActivity))]
        public async Task<User> RunActivity(
            [ActivityTrigger] Guid userId)
        {
            return await usersRepository
                .GetAsync(userId)
                .ConfigureAwait(false);
        }
    }
}
