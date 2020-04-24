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

            var (deploymentActivityName, deploymentActivityInput, deploymentResourceId, deploymentDelete) = functionContext.GetInput<(string, object, string, bool)>();
            var deploymentLog = functionContext.CreateReplaySafeLogger(log ?? NullLogger.Instance);

            IReadOnlyDictionary<string, object> deploymentOutput = null;

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

                        functionContext.ContinueAsNew((deploymentActivityName, deploymentActivityInput, deploymentResourceId, false));
                    }
                }
                else if (deploymentDelete)
                {
                    var state = await functionContext
                        .CallActivityWithRetryAsync<AzureDeploymentState>(nameof(AzureDeploymentStateActivity), deploymentResourceId)
                        .ConfigureAwait(true);

                    if (state.IsErrorState())
                    {
                        // deployments ended up in an error state will stay 
                        // alive for seven days to investigate the issue

                        var schedule = functionContext.CurrentUtcDateTime.AddDays(7).Date;

                        functionContext.SetCustomStatus($"Deployment delete scheduled for '{schedule}'", deploymentLog);

                        await functionContext
                            .CreateTimer(schedule, CancellationToken.None)
                            .ConfigureAwait(true);
                    }

                    functionContext.SetCustomStatus($"Deleting deployment '{deploymentResourceId}'", deploymentLog);

                    await functionContext
                        .CallActivityWithRetryAsync(nameof(AzureDeploymentDeleteActivity), deploymentResourceId)
                        .ConfigureAwait(true);
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
                        functionContext.ContinueAsNew((deploymentActivityName, deploymentActivityInput, deploymentResourceId));
                    }
                    else
                    {
                        try
                        {
                            if (state.IsErrorState())
                            {
                                var errors = await functionContext
                                    .CallActivityWithRetryAsync<IEnumerable<string>>(nameof(AzureDeploymentErrorsActivity), deploymentResourceId)
                                    .ConfigureAwait(true);

                                throw new AzureDeploymentException($"Deployment '{deploymentResourceId}' failed", deploymentResourceId, errors?.ToArray() ?? Array.Empty<string>());
                            }
                            else
                            {
                                var output = await functionContext
                                    .CallActivityWithRetryAsync<IReadOnlyDictionary<string, object>>(nameof(AzureDeploymentOutputActivity), deploymentResourceId)
                                    .ConfigureAwait(true);

                                functionContext.SetOutput(output);
                            }
                        }
                        finally
                        {
                            functionContext.SetCustomStatus($"Initiate deployment clean up for '{deploymentResourceId}'", deploymentLog);

                            functionContext.StartNewOrchestration(nameof(AzureDeploymentOrchestration), (deploymentActivityName, deploymentActivityInput, deploymentResourceId, true));
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                deploymentLog.LogError(exc, $"Orchestration '{nameof(AzureDeploymentOrchestration)}' failed: {exc.Message}");

                throw exc.AsSerializable();
            }
            finally
            {
                functionContext.SetOutput(deploymentOutput);
            }
        }
    }
}
