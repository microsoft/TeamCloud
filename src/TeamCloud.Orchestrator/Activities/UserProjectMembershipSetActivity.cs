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
        private readonly IUsersRepository usersRepository;

        public UserProjectMembershipSetActivity(IUsersRepository usersRepository)
        {
            this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
        }

        [FunctionName(nameof(UserProjectMembershipSetActivity))]
        public async Task<User> RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var (user, projectId) = functionContext.GetInput<(User, string)>();

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
        public static Task<User> SetUserProjectMembershipAsync(this IDurableOrchestrationContext functionContext, User user, string projectId, bool allowUnsafe = false)
            => functionContext.IsLockedBy<User>(user.Id) || allowUnsafe
            ? functionContext.CallActivityWithRetryAsync<User>(nameof(UserProjectMembershipSetActivity), (user, projectId))
            : throw new NotSupportedException($"Unable to create or update project membership without acquired for user {user.Id} lock");
    }
}
