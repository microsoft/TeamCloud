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

            // the project membership should already be on the user at this point
            var membership = user.ProjectMembership(projectId);

            if (membership is null)
                throw new ArgumentException("User must have a ProjectMembership for the project before calling this activity");

            var updatedUser = await usersRepository
                .AddProjectMembershipAsync(user, membership)
                .ConfigureAwait(false);

            return updatedUser;
        }
    }
}
