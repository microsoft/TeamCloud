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

    internal static class UserTeamCloudDetailsSetExtension
    {
        public static Task<User> SetUserTeamCloudInfoAsync(this IDurableOrchestrationContext functionContext, User user, bool allowUnsafe = false)
            => functionContext.IsLockedByContainerDocument(user) || allowUnsafe
            ? functionContext.CallActivityWithRetryAsync<User>(nameof(UserTeamCloudInfoSetActivity), user)
            : throw new NotSupportedException($"Unable to set user '{user.Id}' without acquired lock");
    }
}
