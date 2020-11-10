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
    public class UserOrganizationInfoSetActivity
    {
        private readonly IUserRepository userRepository;

        public UserOrganizationInfoSetActivity(IUserRepository userRepository)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        [FunctionName(nameof(UserOrganizationInfoSetActivity))]
        public async Task<User> RunActivity(
            [ActivityTrigger] IDurableActivityContext activityContext)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            var user = activityContext.GetInput<User>();

            user = await userRepository
                .SetOrganizationInfoAsync(user)
                .ConfigureAwait(false);

            return user;
        }
    }

    internal static class UserOrganizationDetailsSetExtension
    {
        public static Task<User> SetUserOrganizationInfoAsync(this IDurableOrchestrationContext orchestrationContext, User user, bool allowUnsafe = false)
            => orchestrationContext.IsLockedByContainerDocument(user) || allowUnsafe
            ? orchestrationContext.CallActivityWithRetryAsync<User>(nameof(UserOrganizationInfoSetActivity), user)
            : throw new NotSupportedException($"Unable to set user '{user.Id}' without acquired lock");
    }
}
