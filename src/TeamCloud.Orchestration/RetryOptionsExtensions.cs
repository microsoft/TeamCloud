/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace TeamCloud.Orchestration
{
    public static class RetryOptionsExtensions
    {
        private static RetryOptions GetFunctionRetryOptions(IDurableOrchestrationContext orchestration, string functionName, IRetryOptionsFactory retryOptionsFactory = null, Func<Exception, bool> handle = null)
        {
            var factory = retryOptionsFactory ?? RetryOptionsFactory.Default;
            var options = factory.GetRetryOptions(functionName, handle);

            if (!orchestration.IsReplaying)
                Debug.WriteLine($"Function '{functionName}': Calling with a maximum of {options.MaxNumberOfAttempts} attempt/s.");

            return options;
        }

        public static Task CallActivityWithRetryAsync(this IDurableOrchestrationContext orchestration, string functionName, object input)
            => orchestration.CallActivityWithRetryAsync(functionName, GetFunctionRetryOptions(orchestration, functionName), input);

        public static Task CallActivityWithRetryAsync(this IDurableOrchestrationContext orchestration, string functionName, IRetryOptionsFactory retryOptionsFactory, object input)
            => orchestration.CallActivityWithRetryAsync(functionName, GetFunctionRetryOptions(orchestration, functionName, retryOptionsFactory), input);

        public static Task<TResult> CallActivityWithRetryAsync<TResult>(this IDurableOrchestrationContext orchestration, string functionName, object input)
            => orchestration.CallActivityWithRetryAsync<TResult>(functionName, GetFunctionRetryOptions(orchestration, functionName), input);

        public static Task<TResult> CallActivityWithRetryAsync<TResult>(this IDurableOrchestrationContext orchestration, string functionName, IRetryOptionsFactory retryOptionsFactory, object input)
            => orchestration.CallActivityWithRetryAsync<TResult>(functionName, GetFunctionRetryOptions(orchestration, functionName, retryOptionsFactory), input);

        public static Task CallSubOrchestratorWithRetryAsync(this IDurableOrchestrationContext orchestration, string functionName, object input)
            => orchestration.CallSubOrchestratorWithRetryAsync(functionName, GetFunctionRetryOptions(orchestration, functionName), input);

        public static Task CallSubOrchestratorWithRetryAsync(this IDurableOrchestrationContext orchestration, string functionName, IRetryOptionsFactory retryOptionsFactory, object input)
            => orchestration.CallSubOrchestratorWithRetryAsync(functionName, GetFunctionRetryOptions(orchestration, functionName, retryOptionsFactory), input);

        public static Task<TResult> CallSubOrchestratorWithRetryAsync<TResult>(this IDurableOrchestrationContext orchestration, string functionName, object input)
            => orchestration.CallSubOrchestratorWithRetryAsync<TResult>(functionName, GetFunctionRetryOptions(orchestration, functionName), input);

        public static Task<TResult> CallSubOrchestratorWithRetryAsync<TResult>(this IDurableOrchestrationContext orchestration, string functionName, IRetryOptionsFactory retryOptionsFactory, object input)
            => orchestration.CallSubOrchestratorWithRetryAsync<TResult>(functionName, GetFunctionRetryOptions(orchestration, functionName, retryOptionsFactory), input);

        public static Task CallSubOrchestratorWithRetryAsync(this IDurableOrchestrationContext orchestration, string functionName, string instanceId, object input)
            => orchestration.CallSubOrchestratorWithRetryAsync(functionName, GetFunctionRetryOptions(orchestration, functionName), instanceId, input);

        public static Task CallSubOrchestratorWithRetryAsync(this IDurableOrchestrationContext orchestration, string functionName, IRetryOptionsFactory retryOptionsFactory, string instanceId, object input)
            => orchestration.CallSubOrchestratorWithRetryAsync(functionName, GetFunctionRetryOptions(orchestration, functionName, retryOptionsFactory), instanceId, input);

        public static Task<TResult> CallSubOrchestratorWithRetryAsync<TResult>(this IDurableOrchestrationContext orchestration, string functionName, string instanceId, object input)
            => orchestration.CallSubOrchestratorWithRetryAsync<TResult>(functionName, GetFunctionRetryOptions(orchestration, functionName), instanceId, input);

        public static Task<TResult> CallSubOrchestratorWithRetryAsync<TResult>(this IDurableOrchestrationContext orchestration, string functionName, IRetryOptionsFactory retryOptionsFactory, string instanceId, object input)
            => orchestration.CallSubOrchestratorWithRetryAsync<TResult>(functionName, GetFunctionRetryOptions(orchestration, functionName, retryOptionsFactory), instanceId, input);

    }
}
