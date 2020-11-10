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
        private readonly IUserRepository userRepository;

        public UserGetActivity(IUserRepository userRepository)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        [FunctionName(nameof(UserGetActivity))]
        public async Task<User> RunActivity(
            [ActivityTrigger] IDurableActivityContext activityContext,
            ILogger log)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            var input = activityContext.GetInput<Input>();

            try
            {
                var user = await userRepository
                    .GetAsync(input.Organization, input.UserId)
                    .ConfigureAwait(false);

                return user;
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"Failed to resolve user '{input.UserId}': {exc.Message}");

                throw exc.AsSerializable();
            }
        }

        internal struct Input
        {
            public string UserId { get; set; }

            public string Organization { get; set; }
        }
    }

    internal static class UserGetExtensions
    {
        public static Task<User> GetUserAsync(this IDurableOrchestrationContext orchestrationContext, string organizationId, string userId, bool allowUnsafe = false)
            => orchestrationContext.GetUserAsync(organizationId, Guid.Parse(userId), allowUnsafe);

        public static Task<User> GetUserAsync(this IDurableOrchestrationContext orchestrationContext, string organizationId, Guid userId, bool allowUnsafe = false)
            => orchestrationContext.IsLockedBy<User>(userId.ToString()) || allowUnsafe
                ? orchestrationContext.CallActivityWithRetryAsync<User>(nameof(UserGetActivity), new UserGetActivity.Input { Organization = organizationId, UserId = userId.ToString() })
                : throw new NotSupportedException($"Unable to fetch user '{userId}' without acquired lock");
    }
}
