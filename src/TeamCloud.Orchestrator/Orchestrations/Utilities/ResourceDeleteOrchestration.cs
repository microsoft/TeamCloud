/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TeamCloud.Azure.Resources;
using TeamCloud.Orchestration;
using TeamCloud.Orchestration.Deployment;
using TeamCloud.Orchestrator.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities
{
    public static class ResourceDeleteOrchestration
    {
        [FunctionName(nameof(ResourceDeleteOrchestration))]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var functionLog = functionContext.CreateReplaySafeLogger(log ?? NullLogger.Instance);

            var resourceIds = functionContext
                .GetInput<IEnumerable<string>>()
                .ToHashSet(StringComparer.OrdinalIgnoreCase);


            await Task.WhenAll(resourceIds.Select(resourceId =>
            {
                if (AzureResourceIdentifier.TryParse(resourceId, out var resourceIdentifier))
                {
                    if (IsResourceGroup(resourceIdentifier))
                    {
                        functionLog.LogInformation($"Resetting resource group: {resourceId}");

                        return functionContext.CallDeploymentAsync(nameof(ResourceGroupResetActivity), resourceId);
                    }
                    else
                    {
                        functionLog.LogInformation($"Deleting resource: {resourceId}");

                        return functionContext.CallActivityWithRetryAsync(nameof(ResourceDeleteActivity), resourceId);
                    }
                }

                return Task.CompletedTask;

            })).ConfigureAwait(true);

            await Task.WhenAll(resourceIds.Select(resourceId =>
            {
                if (AzureResourceIdentifier.TryParse(resourceId, out var resourceIdentifier) && IsResourceGroup(resourceIdentifier))
                {
                    functionLog.LogInformation($"Deleting resource group: {resourceId}");

                    return functionContext.CallActivityWithRetryAsync(nameof(ResourceGroupDeleteActivity), resourceId);
                }

                return Task.CompletedTask;

            })).ConfigureAwait(true);

            bool IsResourceGroup(AzureResourceIdentifier azureResourceIdentifier)
                => !(azureResourceIdentifier.ResourceTypes?.Any() ?? false);
        }
    }
}
