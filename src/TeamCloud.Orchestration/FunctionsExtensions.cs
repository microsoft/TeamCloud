/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestration;

public static class FunctionsExtensions
{
    public static bool IsActive(this OrchestrationRuntimeStatus status)
        => status == OrchestrationRuntimeStatus.ContinuedAsNew
        || status == OrchestrationRuntimeStatus.Pending
        || status == OrchestrationRuntimeStatus.Running;

    public static bool IsFinal(this OrchestrationRuntimeStatus status)
        => status == OrchestrationRuntimeStatus.Canceled
        || status == OrchestrationRuntimeStatus.Completed
        || status == OrchestrationRuntimeStatus.Failed
        || status == OrchestrationRuntimeStatus.Terminated;

    public static bool IsUnknown(this OrchestrationRuntimeStatus status)
        => status == OrchestrationRuntimeStatus.Unknown;

    public static bool IsFinal(this OrchestrationRuntimeStatus? status)
        => status.GetValueOrDefault(OrchestrationRuntimeStatus.Unknown).IsFinal();

    public static bool IsActive(this OrchestrationRuntimeStatus? status)
        => status.GetValueOrDefault(OrchestrationRuntimeStatus.Unknown).IsActive();

    public static bool IsUnknown(this OrchestrationRuntimeStatus? status)
        => status.GetValueOrDefault(OrchestrationRuntimeStatus.Unknown).IsUnknown();

    private static RetryOptions GetFunctionRetryOptions(IDurableOrchestrationContext orchestration, string functionName, IRetryOptionsFactory retryOptionsFactory = null, Func<Exception, bool> handle = null)
    {
        if (orchestration is null)
            throw new ArgumentNullException(nameof(orchestration));

        var factory = retryOptionsFactory ?? RetryOptionsFactory.Default;
        var options = factory.GetRetryOptions(functionName, handle);

        if (!orchestration.IsReplaying)
            Debug.WriteLine($"Function '{functionName}': Calling with a maximum of {options.MaxNumberOfAttempts} attempt/s.");

        return options;
    }


    public static void SetCustomStatus(this IDurableOrchestrationContext durableOrchestrationContext, object customStatusObject, ILogger log, Exception exception = null)
    {
        if (durableOrchestrationContext is null)
            throw new ArgumentNullException(nameof(durableOrchestrationContext));

        durableOrchestrationContext.SetCustomStatus(customStatusObject);

        var customStatusMessage = customStatusObject is string customStatusString
            ? customStatusString
            : TeamCloudSerialize.SerializeObject(customStatusObject, Formatting.None);

        var message = $"{durableOrchestrationContext.InstanceId}: CUSTOM STATUS -> {customStatusMessage ?? "NULL"}";
        var safeLog = durableOrchestrationContext.CreateReplaySafeLogger(log ?? NullLogger.Instance);

        if (exception is null)
            safeLog.LogInformation(message);
        else
            safeLog.LogError(exception, message);
    }

    public static async Task ContinueAsNew(this IDurableOrchestrationContext orchestration, object input, TimeSpan delay, bool preserveUnprocessedEvents = false)
    {
        if (orchestration is null)
            throw new ArgumentNullException(nameof(orchestration));

        await orchestration
            .CreateTimer(delay)
            .ConfigureAwait(true);

        orchestration.ContinueAsNew(input, preserveUnprocessedEvents);
    }

    public static Task CreateTimer(this IDurableOrchestrationContext orchestration, TimeSpan delay, CancellationToken cancellationToken = default)
        => (orchestration ?? throw new ArgumentNullException(nameof(orchestration)))
        .CreateTimer(orchestration.CurrentUtcDateTime.Add(delay), cancellationToken);

    public static Task CallActivityWithRetryAsync(this IDurableOrchestrationContext orchestration, string functionName, object input)
        => (orchestration ?? throw new ArgumentNullException(nameof(orchestration)))
        .CallActivityWithRetryAsync(functionName, GetFunctionRetryOptions(orchestration, functionName), input);


    public static Task CallActivityWithRetryAsync(this IDurableOrchestrationContext orchestration, string functionName, IRetryOptionsFactory retryOptionsFactory, object input)
        => (orchestration ?? throw new ArgumentNullException(nameof(orchestration)))
        .CallActivityWithRetryAsync(functionName, GetFunctionRetryOptions(orchestration, functionName, retryOptionsFactory), input);


    public static Task<TResult> CallActivityWithRetryAsync<TResult>(this IDurableOrchestrationContext orchestration, string functionName, object input)
        => (orchestration ?? throw new ArgumentNullException(nameof(orchestration)))
        .CallActivityWithRetryAsync<TResult>(functionName, GetFunctionRetryOptions(orchestration, functionName), input);


    public static Task<TResult> CallActivityWithRetryAsync<TResult>(this IDurableOrchestrationContext orchestration, string functionName, IRetryOptionsFactory retryOptionsFactory, object input)
        => (orchestration ?? throw new ArgumentNullException(nameof(orchestration)))
        .CallActivityWithRetryAsync<TResult>(functionName, GetFunctionRetryOptions(orchestration, functionName, retryOptionsFactory), input);


    public static Task CallSubOrchestratorWithRetryAsync(this IDurableOrchestrationContext orchestration, string functionName, object input)
        => (orchestration ?? throw new ArgumentNullException(nameof(orchestration)))
        .CallSubOrchestratorWithRetryAsync(functionName, GetFunctionRetryOptions(orchestration, functionName), input);


    public static Task CallSubOrchestratorWithRetryAsync(this IDurableOrchestrationContext orchestration, string functionName, IRetryOptionsFactory retryOptionsFactory, object input)
        => (orchestration ?? throw new ArgumentNullException(nameof(orchestration)))
        .CallSubOrchestratorWithRetryAsync(functionName, GetFunctionRetryOptions(orchestration, functionName, retryOptionsFactory), input);


    public static Task<TResult> CallSubOrchestratorWithRetryAsync<TResult>(this IDurableOrchestrationContext orchestration, string functionName, object input)
        => (orchestration ?? throw new ArgumentNullException(nameof(orchestration)))
        .CallSubOrchestratorWithRetryAsync<TResult>(functionName, GetFunctionRetryOptions(orchestration, functionName), input);


    public static Task<TResult> CallSubOrchestratorWithRetryAsync<TResult>(this IDurableOrchestrationContext orchestration, string functionName, IRetryOptionsFactory retryOptionsFactory, object input)
        => (orchestration ?? throw new ArgumentNullException(nameof(orchestration)))
        .CallSubOrchestratorWithRetryAsync<TResult>(functionName, GetFunctionRetryOptions(orchestration, functionName, retryOptionsFactory), input);


    public static Task CallSubOrchestratorWithRetryAsync(this IDurableOrchestrationContext orchestration, string functionName, string instanceId, object input)
        => (orchestration ?? throw new ArgumentNullException(nameof(orchestration)))
        .CallSubOrchestratorWithRetryAsync(functionName, GetFunctionRetryOptions(orchestration, functionName), instanceId, input);


    public static Task CallSubOrchestratorWithRetryAsync(this IDurableOrchestrationContext orchestration, string functionName, IRetryOptionsFactory retryOptionsFactory, string instanceId, object input)
        => (orchestration ?? throw new ArgumentNullException(nameof(orchestration)))
        .CallSubOrchestratorWithRetryAsync(functionName, GetFunctionRetryOptions(orchestration, functionName, retryOptionsFactory), instanceId, input);


    public static Task<TResult> CallSubOrchestratorWithRetryAsync<TResult>(this IDurableOrchestrationContext orchestration, string functionName, string instanceId, object input)
        => (orchestration ?? throw new ArgumentNullException(nameof(orchestration)))
        .CallSubOrchestratorWithRetryAsync<TResult>(functionName, GetFunctionRetryOptions(orchestration, functionName), instanceId, input);


    public static Task<TResult> CallSubOrchestratorWithRetryAsync<TResult>(this IDurableOrchestrationContext orchestration, string functionName, IRetryOptionsFactory retryOptionsFactory, string instanceId, object input)
        => (orchestration ?? throw new ArgumentNullException(nameof(orchestration)))
        .CallSubOrchestratorWithRetryAsync<TResult>(functionName, GetFunctionRetryOptions(orchestration, functionName, retryOptionsFactory), instanceId, input);

}
