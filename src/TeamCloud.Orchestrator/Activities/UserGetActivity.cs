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
    public class UserGetActivity
    {
        private readonly IUsersRepository usersRepository;

        public UserGetActivity(IUsersRepository usersRepository)
        {
            this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
        }

        [FunctionName(nameof(UserGetActivity))]
        public async Task<UserDocument> RunActivity(
            [ActivityTrigger] string userId)
        {
            return await usersRepository
                .GetAsync(userId)
                .ConfigureAwait(false);
        }
    }

    internal static class UserGetExtension
    {
        public static Task<UserDocument> GetUserAsync(this IDurableOrchestrationContext functionContext, string userId, bool allowUnsafe = false)
            => functionContext.IsLockedBy<UserDocument>(userId) || allowUnsafe
            ? functionContext.CallActivityWithRetryAsync<UserDocument>(nameof(UserGetActivity), userId)
            : throw new NotSupportedException($"Unable to get user '{userId}' without acquired lock");
    }
}
