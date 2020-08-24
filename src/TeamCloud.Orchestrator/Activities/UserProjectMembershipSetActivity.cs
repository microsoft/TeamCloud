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
        private readonly IUserRepository usersRepository;

        public UserProjectMembershipSetActivity(IUserRepository usersRepository)
        {
            this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
        }

        [FunctionName(nameof(UserProjectMembershipSetActivity))]
        public async Task<UserDocument> RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var (user, projectId) = functionContext.GetInput<(UserDocument, string)>();

            var membership = user.ProjectMembership(projectId);

            if (membership is null)
                throw new InvalidOperationException($"User must contain a ProjectMembership for Project '{projectId}'");

            user = await usersRepository
                .AddProjectMembershipAsync(user, membership)
                .ConfigureAwait(false);

            return user;
        }
    }

    internal static class UserProjectMembershipSetExtension
    {
        public static Task<UserDocument> SetUserProjectMembershipAsync(this IDurableOrchestrationContext functionContext, UserDocument user, string projectId, bool allowUnsafe = false)
            => functionContext.IsLockedBy<UserDocument>(user.Id) || allowUnsafe
            ? functionContext.CallActivityWithRetryAsync<UserDocument>(nameof(UserProjectMembershipSetActivity), (user, projectId))
            : throw new NotSupportedException($"Unable to create or update project membership without acquired lock for user {user.Id}");
    }
}
