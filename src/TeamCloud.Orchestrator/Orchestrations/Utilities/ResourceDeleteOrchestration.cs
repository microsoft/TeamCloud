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
            [OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,
            ILogger log)
        {
            if (orchestrationContext is null)
                throw new ArgumentNullException(nameof(orchestrationContext));

            var functionLog = orchestrationContext.CreateReplaySafeLogger(log ?? NullLogger.Instance);

            var resourceIds = orchestrationContext
                .GetInput<IEnumerable<string>>()
                .ToHashSet(StringComparer.OrdinalIgnoreCase);


            await Task.WhenAll(resourceIds.Select(resourceId =>
            {
                if (AzureResourceIdentifier.TryParse(resourceId, out var resourceIdentifier))
                {
                    if (IsResourceGroup(resourceIdentifier))
                    {
                        functionLog.LogInformation($"Resetting resource group: {resourceId}");

                        return orchestrationContext.CallDeploymentAsync(nameof(ResourceGroupResetActivity), resourceId);
                    }
                    else
                    {
                        functionLog.LogInformation($"Deleting resource: {resourceId}");

                        return orchestrationContext.CallActivityWithRetryAsync(nameof(ResourceDeleteActivity), resourceId);
                    }
                }

                return Task.CompletedTask;

            })).ConfigureAwait(true);

            await Task.WhenAll(resourceIds.Select(resourceId =>
            {
                if (AzureResourceIdentifier.TryParse(resourceId, out var resourceIdentifier) && IsResourceGroup(resourceIdentifier))
                {
                    functionLog.LogInformation($"Deleting resource group: {resourceId}");

                    return orchestrationContext.CallActivityWithRetryAsync(nameof(ResourceGroupDeleteActivity), resourceId);
                }

                return Task.CompletedTask;

            })).ConfigureAwait(true);

            static bool IsResourceGroup(AzureResourceIdentifier azureResourceIdentifier)
                => !(azureResourceIdentifier.ResourceTypes?.Any() ?? false);
        }
    }

    internal static class ResourceDeleteExtensions
    {
        internal static Task DeleteResourcesAsync(this IDurableOrchestrationContext orchestrationContext, bool wait, params string[] resourceIds)
        {
            if (orchestrationContext is null)
                throw new ArgumentNullException(nameof(orchestrationContext));

            resourceIds = resourceIds
                .Where(resourceId => !string.IsNullOrEmpty(resourceId))
                .ToArray();

            if (resourceIds.Any())
            {
                if (wait)
                    return orchestrationContext.CallSubOrchestratorAsync(nameof(ResourceDeleteOrchestration), resourceIds.ToHashSet(StringComparer.OrdinalIgnoreCase));

                orchestrationContext.StartNewOrchestration(nameof(ResourceDeleteOrchestration), resourceIds.ToHashSet(StringComparer.OrdinalIgnoreCase));
            }

            return Task.CompletedTask;
        }
    }
}
