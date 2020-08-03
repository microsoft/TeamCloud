/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Internal.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Entities;

namespace TeamCloud.Orchestrator.Activities
{
    public class UserProjectMembershipDeleteActivity
    {
        private readonly IUsersRepository usersRepository;

        public UserProjectMembershipDeleteActivity(IUsersRepository usersRepository)
        {
            this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
        }

        [FunctionName(nameof(UserProjectMembershipDeleteActivity))]
        public async Task<UserDocument> RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var (user, projectId) = functionContext.GetInput<(UserDocument, string)>();

            var updatedUser = await usersRepository
                .RemoveProjectMembershipAsync(user, projectId)
                .ConfigureAwait(false);

            return updatedUser;
        }
    }

    internal static class UserProjectMembershipDeleteExtension
    {
        public static Task<UserDocument> DeleteUserProjectMembershipAsync(this IDurableOrchestrationContext functionContext, UserDocument user, string projectId, bool allowUnsafe = false)
            => functionContext.IsLockedBy<UserDocument>(user.Id) || allowUnsafe
            ? functionContext.CallActivityWithRetryAsync<UserDocument>(nameof(UserProjectMembershipDeleteActivity), (user, projectId))
            : throw new NotSupportedException($"Unable to delete project membership without acquired for user {user.Id} lock");
    }
}
