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
        public static Task<UserDocument> GetUserAsync(this IDurableOrchestrationContext functionContext, string userId, bool allowUnsafe = false)
            => functionContext.IsLockedBy<UserDocument>(userId) || allowUnsafe
            ? functionContext.CallActivityWithRetryAsync<UserDocument>(nameof(UserGetActivity), userId)
            : throw new NotSupportedException($"Unable to get user '{userId}' without acquired lock");
    }
}
