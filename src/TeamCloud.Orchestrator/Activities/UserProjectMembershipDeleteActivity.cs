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
    public class UserProjectMembershipDeleteActivity
    {
        private readonly IUserRepository userRepository;

        public UserProjectMembershipDeleteActivity(IUserRepository userRepository)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        [FunctionName(nameof(UserProjectMembershipDeleteActivity))]
        public async Task<User> RunActivity(
            [ActivityTrigger] IDurableActivityContext activityContext)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            var functionInput = activityContext.GetInput<Input>();

            var updatedUser = await userRepository
                .RemoveProjectMembershipAsync(functionInput.User, functionInput.ProjectId)
                .ConfigureAwait(false);

            return updatedUser;
        }

        internal struct Input
        {
            public User User { get; set; }

            public string ProjectId { get; set; }
        }

    }

    internal static class UserProjectMembershipDeleteExtension
    {
        public static Task<User> DeleteUserProjectMembershipAsync(this IDurableOrchestrationContext orchestrationContext, User user, string projectId, bool allowUnsafe = false)
            => orchestrationContext.IsLockedBy<User>(user.Id) || allowUnsafe
            ? orchestrationContext.CallActivityWithRetryAsync<User>(nameof(UserProjectMembershipDeleteActivity), new UserProjectMembershipDeleteActivity.Input() { User = user, ProjectId = projectId })
            : throw new NotSupportedException($"Unable to delete project membership without acquired for user {user.Id} lock");
    }
}
