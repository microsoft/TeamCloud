/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Activities
{
    internal static class UserProjectMembershipSetExtension
    {
        public static Task<User> SetUserProjectMembershipAsync(this IDurableOrchestrationContext functionContext, User user, string projectId)
            => functionContext.CallActivityWithRetryAsync<User>(nameof(UserProjectMembershipSetActivity), (user, projectId));
    }
}
