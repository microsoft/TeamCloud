/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace TeamCloud.Orchestration
{
    public static class RetryOptionsExtensions
    {
        private static RetryOptions GetFunctionRetryOptions(string functionName)
        {
            var retryAttribute = RetryOptionsAttribute.GetByFunctionName(functionName) ?? new RetryOptionsAttribute(1);

            var retryOptions = new RetryOptions(TimeSpan.Parse(retryAttribute.FirstRetryInterval), retryAttribute.MaxNumberOfAttempts)
            {
                MaxRetryInterval = TimeSpan.Parse(retryAttribute.MaxRetryInterval),
                RetryTimeout = TimeSpan.Parse(retryAttribute.RetryTimeout),
                BackoffCoefficient = retryAttribute.BackoffCoefficient
            };

            return retryOptions;
        }

        public static Task CallActivityWithRetryAsync(this IDurableOrchestrationContext orchestration, string functionName, object input)
            => orchestration.CallActivityWithRetryAsync(functionName, GetFunctionRetryOptions(functionName), input);

        public static Task<TResult> CallActivityWithRetryAsync<TResult>(this IDurableOrchestrationContext orchestration, string functionName, object input)
            => orchestration.CallActivityWithRetryAsync<TResult>(functionName, GetFunctionRetryOptions(functionName), input);

        public static Task CallSubOrchestratorWithRetryAsync(this IDurableOrchestrationContext orchestration, string functionName, object input)
            => orchestration.CallSubOrchestratorWithRetryAsync(functionName, GetFunctionRetryOptions(functionName), input);

        public static Task<TResult> CallSubOrchestratorWithRetryAsync<TResult>(this IDurableOrchestrationContext orchestration, string functionName, object input)
            => orchestration.CallSubOrchestratorWithRetryAsync<TResult>(functionName, GetFunctionRetryOptions(functionName), input);

        public static Task CallSubOrchestratorWithRetryAsync(this IDurableOrchestrationContext orchestration, string functionName, string instanceId, object input)
            => orchestration.CallSubOrchestratorWithRetryAsync(functionName, GetFunctionRetryOptions(functionName), instanceId, input);

        public static Task<TResult> CallSubOrchestratorWithRetryAsync<TResult>(this IDurableOrchestrationContext orchestration, string functionName, string instanceId, object input)
            => orchestration.CallSubOrchestratorWithRetryAsync<TResult>(functionName, GetFunctionRetryOptions(functionName), instanceId, input);

    }
}
