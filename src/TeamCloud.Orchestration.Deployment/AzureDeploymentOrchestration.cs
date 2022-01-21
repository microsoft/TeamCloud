/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TeamCloud.Azure.Deployment;
using TeamCloud.Orchestration.Deployment.Activities;
using TeamCloud.Orchestration.Eventing;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestration.Deployment;

public static class AzureDeploymentOrchestration
{
    [FunctionName(nameof(AzureDeploymentOrchestration)), RetryOptions(3)]
    public static async Task RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,

        ILogger log)
    {
        if (orchestrationContext is null)
            throw new ArgumentNullException(nameof(orchestrationContext));

        var functionInput = orchestrationContext.GetInput<Input>();
        var functionLog = orchestrationContext.CreateReplaySafeLogger(log ?? NullLogger.Instance);

        try
        {
            if (string.IsNullOrEmpty(functionInput.DeploymentResourceId))
            {
                orchestrationContext.SetCustomStatus($"Starting deployment using activity '{functionInput.DeploymentActivityName}'", functionLog);

                functionInput.DeploymentResourceId = await orchestrationContext
                    .CallActivityWithRetryAsync<string>(functionInput.DeploymentActivityName, functionInput.DeploymentActivityInput)
                    .ConfigureAwait(true);

                if (!string.IsNullOrEmpty(functionInput.DeploymentResourceId))
                {
                    orchestrationContext.SetCustomStatus($"Monitoring deployment '{functionInput.DeploymentResourceId}'", functionLog);

                    orchestrationContext.ContinueAsNew(functionInput);
                }
            }
            else
            {
                await orchestrationContext
                    .CreateTimer(orchestrationContext.CurrentUtcDateTime.AddSeconds(10), CancellationToken.None)
                    .ConfigureAwait(true);

                var state = await orchestrationContext
                    .CallActivityWithRetryAsync<AzureDeploymentState>(nameof(AzureDeploymentStateActivity), functionInput.DeploymentResourceId)
                    .ConfigureAwait(true);

                if (state.IsProgressState())
                {
                    orchestrationContext
                        .ContinueAsNew(functionInput);
                }
                else if (state.IsErrorState())
                {
                    var errors = (await orchestrationContext
                        .CallActivityWithRetryAsync<IEnumerable<string>>(nameof(AzureDeploymentErrorsActivity), functionInput.DeploymentResourceId)
                        .ConfigureAwait(true)) ?? Enumerable.Empty<string>();

                    foreach (var error in errors)
                        functionLog.LogError($"Deployment '{functionInput.DeploymentResourceId}' reported error: {error}");

                    throw new AzureDeploymentException($"Deployment '{functionInput.DeploymentResourceId}' failed", functionInput.DeploymentResourceId, errors.ToArray());
                }
                else
                {
                    var output = await orchestrationContext
                        .GetDeploymentOutputAsync(functionInput.DeploymentResourceId)
                        .ConfigureAwait(true);

                    if (!string.IsNullOrEmpty(functionInput.DeploymentOutputEventName))
                    {
                        await orchestrationContext
                            .RaiseEventAsync(functionInput.DeploymentOwnerInstanceId, functionInput.DeploymentOutputEventName, output)
                            .ConfigureAwait(true);
                    }

                    orchestrationContext.SetOutput(output);
                }
            }
        }
        catch (Exception exc)
        {
            functionLog.LogError(exc, $"Orchestration '{nameof(AzureDeploymentOrchestration)}' failed: {exc.Message}");

            throw exc.AsSerializable();
        }
    }

    internal struct Input
    {
        public string DeploymentOwnerInstanceId { get; set; }

        public string DeploymentActivityName { get; set; }

        public object DeploymentActivityInput { get; set; }

        public string DeploymentResourceId { get; set; }

        public string DeploymentOutputEventName { get; set; }
    }

}
