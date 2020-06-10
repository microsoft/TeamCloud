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
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Entities;

namespace TeamCloud.Orchestrator.Activities
{
    public class UserDeleteActivity
    {
        private readonly IUsersRepository usersRepository;

        public UserDeleteActivity(IUsersRepository usersRepository)
        {
            this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
        }

        [FunctionName(nameof(UserDeleteActivity))]
        public async Task RunActivity(
            [ActivityTrigger] User user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            _ = await usersRepository
                .RemoveAsync(user)
                .ConfigureAwait(false);
        }
    }

    internal static class UserDeleteExtension
    {
        public static Task<User> DeleteUserAsync(this IDurableOrchestrationContext functionContext, string userId, bool allowUnsafe = false)
            => DeleteUserAsync(functionContext, Guid.Parse(userId), allowUnsafe);

        public static Task<User> DeleteUserAsync(this IDurableOrchestrationContext functionContext, Guid userId, bool allowUnsafe = false)
            => functionContext.IsLockedBy<User>(userId.ToString()) || allowUnsafe
            ? functionContext.CallActivityWithRetryAsync<User>(nameof(UserDeleteActivity), userId)
            : throw new NotSupportedException($"Unable to delete user '{userId}' without acquired lock");
    }
}
