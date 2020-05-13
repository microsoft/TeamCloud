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
    internal static class UserSetExtension
    {
        public static Task<User> SetUserAsync(this IDurableOrchestrationContext functionContext, User user, bool allowUnsafe = false)
            => functionContext.IsLockedByContainerDocument(user) || allowUnsafe
            ? functionContext.CallActivityWithRetryAsync<User>(nameof(UserSetActivity), user)
            : throw new NotSupportedException($"Unable to set user '{user.Id}' without acquired lock");
    }
}
