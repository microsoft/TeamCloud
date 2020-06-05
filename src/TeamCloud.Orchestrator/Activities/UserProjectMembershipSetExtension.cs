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
    internal static class UserProjectMembershipSetExtension
    {
        public static Task<User> SetUserProjectMembershipAsync(this IDurableOrchestrationContext functionContext, User user, string projectId, bool allowUnsafe = false)
            => functionContext.IsLockedBy<User>(user.Id) || allowUnsafe
            ? functionContext.CallActivityWithRetryAsync<User>(nameof(UserProjectMembershipSetActivity), (user, projectId))
            : throw new NotSupportedException($"Unable to create or update project membership without acquired for user {user.Id} lock");
    }
}
