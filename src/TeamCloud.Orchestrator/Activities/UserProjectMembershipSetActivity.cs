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
    public class UserProjectMembershipSetActivity
    {
        private readonly IUserRepository userRepository;

        public UserProjectMembershipSetActivity(IUserRepository userRepository)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        [FunctionName(nameof(UserProjectMembershipSetActivity))]
        public async Task<UserDocument> RunActivity(
            [ActivityTrigger] IDurableActivityContext activityContext)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            var functionInput = activityContext.GetInput<Input>();

            var membership = functionInput.User.ProjectMembership(functionInput.ProjectId);

            if (membership is null)
                throw new InvalidOperationException($"User must contain a ProjectMembership for Project '{functionInput.ProjectId}'");

            functionInput.User = await userRepository
                .AddProjectMembershipAsync(functionInput.User, membership)
                .ConfigureAwait(false);

            return functionInput.User;
        }

        internal struct Input
        {
            public UserDocument User { get; set; }

            public string ProjectId { get; set; }
        }

    }

    internal static class UserProjectMembershipSetExtension
    {
        public static Task<UserDocument> SetUserProjectMembershipAsync(this IDurableOrchestrationContext orchestrationContext, UserDocument user, string projectId, bool allowUnsafe = false)
            => orchestrationContext.IsLockedBy<UserDocument>(user.Id) || allowUnsafe
            ? orchestrationContext.CallActivityWithRetryAsync<UserDocument>(nameof(UserProjectMembershipSetActivity), new UserProjectMembershipSetActivity.Input() { User = user, ProjectId = projectId })
            : throw new NotSupportedException($"Unable to create or update project membership without acquired lock for user {user.Id}");
    }
}
