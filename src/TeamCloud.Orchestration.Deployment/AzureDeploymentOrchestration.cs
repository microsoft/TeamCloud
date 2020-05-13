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

namespace TeamCloud.Orchestration.Deployment
{
    public static class AzureDeploymentOrchestration
    {
        [FunctionName(nameof(AzureDeploymentOrchestration)), RetryOptions(3)]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,

            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            (string deploymentOwnerInstanceId, string deploymentActivityName, object deploymentActivityInput, string deploymentResourceId, string deploymentOutputEventName)
                = functionContext.GetInput<(string, string, object, string, string)>();

            var deploymentLog = functionContext.CreateReplaySafeLogger(log ?? NullLogger.Instance);

            try
            {
                if (string.IsNullOrEmpty(deploymentResourceId))
                {
                    functionContext.SetCustomStatus($"Starting deployment using activity '{deploymentActivityName}'", deploymentLog);

                    deploymentResourceId = await functionContext
                        .CallActivityWithRetryAsync<string>(deploymentActivityName, deploymentActivityInput)
                        .ConfigureAwait(true);

                    if (!string.IsNullOrEmpty(deploymentResourceId))
                    {
                        functionContext.SetCustomStatus($"Monitoring deployment '{deploymentResourceId}'", deploymentLog);

                        functionContext.ContinueAsNew((deploymentOwnerInstanceId, deploymentActivityName, deploymentActivityInput, deploymentResourceId, deploymentOutputEventName));
                    }
                }
                else
                {
                    await functionContext
                        .CreateTimer(functionContext.CurrentUtcDateTime.AddSeconds(10), CancellationToken.None)
                        .ConfigureAwait(true);

                    var state = await functionContext
                        .CallActivityWithRetryAsync<AzureDeploymentState>(nameof(AzureDeploymentStateActivity), deploymentResourceId)
                        .ConfigureAwait(true);

                    if (state.IsProgressState())
                    {
                        functionContext
                            .ContinueAsNew((deploymentOwnerInstanceId, deploymentActivityName, deploymentActivityInput, deploymentResourceId, deploymentOutputEventName));
                    }
                    else if (state.IsErrorState())
                    {
                        var errors = (await functionContext
                            .CallActivityWithRetryAsync<IEnumerable<string>>(nameof(AzureDeploymentErrorsActivity), deploymentResourceId)
                            .ConfigureAwait(true)) ?? Enumerable.Empty<string>();

                        foreach (var error in errors)
                            deploymentLog.LogError($"Deployment '{deploymentResourceId}' reported error: {error}");

                        throw new AzureDeploymentException($"Deployment '{deploymentResourceId}' failed", deploymentResourceId, errors.ToArray());
                    }
                    else
                    {
                        var output = await functionContext
                            .GetDeploymentOutputAsync(deploymentResourceId)
                            .ConfigureAwait(true);

                        if (!string.IsNullOrEmpty(deploymentOutputEventName))
                        {
                            await functionContext
                                .RaiseEventAsync(deploymentOwnerInstanceId, deploymentOutputEventName, output)
                                .ConfigureAwait(true);
                        }

                        functionContext.SetOutput(output);
                    }
                }
            }
            catch (Exception exc)
            {
                deploymentLog.LogError(exc, $"Orchestration '{nameof(AzureDeploymentOrchestration)}' failed: {exc.Message}");

                throw exc.AsSerializable();
            }
        }
    }
}
