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
        public async Task<User> RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var (user, projectId) = functionContext.GetInput<(User, string)>();

            var updatedUser = await usersRepository
                .RemoveProjectMembershipAsync(user, projectId)
                .ConfigureAwait(false);

            return updatedUser;
        }
    }

    internal static class UserProjectMembershipDeleteExtension
    {
        public static Task<User> DeleteUserProjectMembershipAsync(this IDurableOrchestrationContext functionContext, User user, string projectId)
            => functionContext.CallActivityWithRetryAsync<User>(nameof(UserProjectMembershipDeleteActivity), (user, projectId));
    }
}
