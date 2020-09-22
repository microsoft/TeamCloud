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
        private readonly IUserRepository usersRepository;

        public UserDeleteActivity(IUserRepository usersRepository)
        {
            this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
        }

        [FunctionName(nameof(UserDeleteActivity))]
        public async Task RunActivity(
            [ActivityTrigger] UserDocument user)
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
        public static Task<UserDocument> DeleteUserAsync(this IDurableOrchestrationContext orchestrationContext, string userId, bool allowUnsafe = false)
            => DeleteUserAsync(orchestrationContext, Guid.Parse(userId), allowUnsafe);

        public static async Task<UserDocument> DeleteUserAsync(this IDurableOrchestrationContext orchestrationContext, Guid userId, bool allowUnsafe = false)
        {
            var user = await orchestrationContext
                .GetUserAsync(userId, true)
                .ConfigureAwait(true);

            return await orchestrationContext
                .DeleteUserAsync(user, allowUnsafe)
                .ConfigureAwait(true);
        }

        public static Task<UserDocument> DeleteUserAsync(this IDurableOrchestrationContext orchestrationContext, UserDocument user, bool allowUnsafe = false)
            => orchestrationContext.IsLockedBy<UserDocument>(user.Id.ToString()) || allowUnsafe
                ? orchestrationContext.CallActivityWithRetryAsync<UserDocument>(nameof(UserDeleteActivity), user)
                : throw new NotSupportedException($"Unable to delete user '{user.Id}' without acquired lock");
    }
}
