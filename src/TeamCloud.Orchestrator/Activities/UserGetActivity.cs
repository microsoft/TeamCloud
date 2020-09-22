/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Entities;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Activities
{
    public sealed class UserGetActivity
    {
        private readonly IUserRepository usersRepository;

        public UserGetActivity(IUserRepository usersRepository)
        {
            this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
        }

        [FunctionName(nameof(UserGetActivity))]
        public async Task<UserDocument> RunActivity(
            [ActivityTrigger] IDurableActivityContext activityContext,
            ILogger log)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            var userId = activityContext.GetInput<string>();

            try
            {
                var user = await usersRepository
                    .GetAsync(userId)
                    .ConfigureAwait(false);

                return user;
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"Failed to resolve user '{userId}': {exc.Message}");

                throw exc.AsSerializable();
            }
        }
    }

    internal static class UserGetExtensions
    {
        public static Task<UserDocument> GetUserAsync(this IDurableOrchestrationContext orchestrationContext, string userId, bool allowUnsafe = false)
            => orchestrationContext.GetUserAsync(Guid.Parse(userId), allowUnsafe);

        public static Task<UserDocument> GetUserAsync(this IDurableOrchestrationContext orchestrationContext, Guid userId, bool allowUnsafe = false)
            => orchestrationContext.IsLockedBy<UserDocument>(userId.ToString()) || allowUnsafe
                ? orchestrationContext.CallActivityWithRetryAsync<UserDocument>(nameof(UserGetActivity), userId.ToString())
                : throw new NotSupportedException($"Unable to fetch user '{userId}' without acquired lock");
    }
}
