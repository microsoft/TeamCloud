/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Entities;

namespace TeamCloud.Orchestrator.Activities
{
    internal static class UserProjectMembershipDeleteExtension
    {
        public static Task<User> DeleteUserProjectMembershipAsync(this IDurableOrchestrationContext functionContext, User user, string projectId, bool allowUnsafe = false)
            => functionContext.IsLockedBy<User>(user.Id) || allowUnsafe
            ? functionContext.CallActivityWithRetryAsync<User>(nameof(UserProjectMembershipDeleteActivity), (user, projectId))
            : throw new NotSupportedException($"Unable to delete project membership without acquired for user {user.Id} lock");
    }
}
