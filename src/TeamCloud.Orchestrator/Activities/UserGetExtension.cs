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
    internal static class UserGetExtension
    {
        public static Task<User> GetUserAsync(this IDurableOrchestrationContext functionContext, string userId, bool allowUnsafe = false)
            => functionContext.IsLockedBy<User>(userId) || allowUnsafe
            ? functionContext.CallActivityWithRetryAsync<User>(nameof(UserGetActivity), userId)
            : throw new NotSupportedException($"Unable to get user '{userId}' without acquired lock");
    }
}
