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
using TeamCloud.Azure.Deployment;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Orchestrations.Utilities.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities
{
    public static class DeploymentOutputOrchestration
    {
        [FunctionName(nameof(DeploymentOutputOrchestration))]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var resourceId = functionContext.GetInput<string>();

            await functionContext
                .CreateTimer(functionContext.CurrentUtcDateTime.AddSeconds(10), CancellationToken.None)
                .ConfigureAwait(true);

            var state = await functionContext
                .CallActivityWithRetryAsync<AzureDeploymentState>(nameof(DeploymentStateActivity), resourceId)
                .ConfigureAwait(true);

            if (state.IsProgressState())
            {
                functionContext.ContinueAsNew(resourceId);
            }
            else if (state.IsErrorState())
            {
                var errors = await functionContext
                    .CallActivityWithRetryAsync<IEnumerable<string>>(nameof(DeploymentErrorsActivity), resourceId)
                    .ConfigureAwait(true);

                throw new AzureDeploymentException($"Deployment '{resourceId}' failed", resourceId, errors?.ToArray() ?? Array.Empty<string>());
            }
            else
            {
                var output = await functionContext
                    .CallActivityWithRetryAsync<IReadOnlyDictionary<string, object>>(nameof(DeploymentOutputActivity), resourceId)
                    .ConfigureAwait(true);

                functionContext.SetOutput(output);
            }
        }
    }
}
