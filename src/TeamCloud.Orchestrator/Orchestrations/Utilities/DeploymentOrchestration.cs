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
using TeamCloud.Azure.Deployment;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Orchestrations.Utilities.Activities;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities
{
    public static class DeploymentOrchestration
    {
        [FunctionName(nameof(DeploymentOrchestration)), RetryOptions(3)]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var (deploymentActivityName, deploymentActivityInput, deploymentResourceId) = functionContext.GetInput<(string, object, string)>();

            try
            {
                if (string.IsNullOrEmpty(deploymentResourceId))
                {
                    deploymentResourceId = await functionContext
                        .CallActivityWithRetryAsync<string>(deploymentActivityName, deploymentActivityInput)
                        .ConfigureAwait(true);

                    if (string.IsNullOrEmpty(deploymentResourceId))
                    {
                        functionContext.SetOutput(null);
                    }
                    else
                    {
                        functionContext.ContinueAsNew((deploymentActivityName, deploymentActivityInput, deploymentResourceId));
                    }
                }
                else
                {
                    await functionContext
                        .CreateTimer(functionContext.CurrentUtcDateTime.AddSeconds(10), CancellationToken.None)
                        .ConfigureAwait(true);

                    var state = await functionContext
                        .CallActivityWithRetryAsync<AzureDeploymentState>(nameof(DeploymentStateActivity), deploymentResourceId)
                        .ConfigureAwait(true);

                    if (state.IsProgressState())
                    {
                        functionContext.ContinueAsNew((deploymentActivityName, deploymentActivityInput, deploymentResourceId));
                    }
                    else if (state.IsErrorState())
                    {
                        var errors = await functionContext
                            .CallActivityWithRetryAsync<IEnumerable<string>>(nameof(DeploymentErrorsActivity), deploymentResourceId)
                            .ConfigureAwait(true);

                        throw new AzureDeploymentException($"Deployment '{deploymentResourceId}' failed", deploymentResourceId, errors?.ToArray() ?? Array.Empty<string>());
                    }
                    else
                    {
                        var output = await functionContext
                            .CallActivityWithRetryAsync<IReadOnlyDictionary<string, object>>(nameof(DeploymentOutputActivity), deploymentResourceId)
                            .ConfigureAwait(true);

                        functionContext.SetOutput(output);
                    }
                }
            }
            catch (Exception exc) when (!exc.IsSerializable(out var serializableException))
            {
                log.LogError(exc, $"Orchestration '{nameof(DeploymentOrchestration)}' failed: {exc.Message}");

                throw serializableException;
            }
        }
    }
}
