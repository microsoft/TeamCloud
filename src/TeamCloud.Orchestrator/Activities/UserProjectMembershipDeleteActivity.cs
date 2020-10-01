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
        private readonly IUserRepository usersRepository;

        public UserProjectMembershipDeleteActivity(IUserRepository usersRepository)
        {
            this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
        }

        [FunctionName(nameof(UserProjectMembershipDeleteActivity))]
        public async Task<UserDocument> RunActivity(
            [ActivityTrigger] IDurableActivityContext activityContext)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            var functionInput = activityContext.GetInput<Input>();

            var updatedUser = await usersRepository
                .RemoveProjectMembershipAsync(functionInput.User, functionInput.ProjectId)
                .ConfigureAwait(false);

            return updatedUser;
        }

        internal struct Input
        {
            public UserDocument User { get; set; }

            public string ProjectId { get; set; }
        }

    }

    internal static class UserProjectMembershipDeleteExtension
    {
        public static Task<UserDocument> DeleteUserProjectMembershipAsync(this IDurableOrchestrationContext orchestrationContext, UserDocument user, string projectId, bool allowUnsafe = false)
            => orchestrationContext.IsLockedBy<UserDocument>(user.Id) || allowUnsafe
            ? orchestrationContext.CallActivityWithRetryAsync<UserDocument>(nameof(UserProjectMembershipDeleteActivity), new UserProjectMembershipDeleteActivity.Input() { User = user, ProjectId = projectId })
            : throw new NotSupportedException($"Unable to delete project membership without acquired for user {user.Id} lock");
    }
}
