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
        private readonly IUserRepository userRepository;

        public UserDeleteActivity(IUserRepository userRepository)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        [FunctionName(nameof(UserDeleteActivity))]
        public async Task RunActivity(
            [ActivityTrigger] User user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            _ = await userRepository
                .RemoveAsync(user)
                .ConfigureAwait(false);
        }
    }

    internal static class UserDeleteExtension
    {
        public static Task<User> DeleteUserAsync(this IDurableOrchestrationContext orchestrationContext, string organizationId, string userId, bool allowUnsafe = false)
            => DeleteUserAsync(orchestrationContext, organizationId, Guid.Parse(userId), allowUnsafe);

        public static async Task<User> DeleteUserAsync(this IDurableOrchestrationContext orchestrationContext, string organizationId, Guid userId, bool allowUnsafe = false)
        {
            var user = await orchestrationContext
                .GetUserAsync(organizationId, userId, true)
                .ConfigureAwait(true);

            return await orchestrationContext
                .DeleteUserAsync(organizationId, user, allowUnsafe)
                .ConfigureAwait(true);
        }

        public static Task<User> DeleteUserAsync(this IDurableOrchestrationContext orchestrationContext, string organization, User user, bool allowUnsafe = false)
            => orchestrationContext.IsLockedBy<User>(user.Id.ToString()) || allowUnsafe
                ? orchestrationContext.CallActivityWithRetryAsync<User>(nameof(UserDeleteActivity), user)
                : throw new NotSupportedException($"Unable to delete user '{user.Id}' without acquired lock");
    }
}
