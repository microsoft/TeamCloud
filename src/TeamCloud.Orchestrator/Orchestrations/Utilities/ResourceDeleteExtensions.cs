/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities
{
    internal static class ResourceDeleteExtensions
    {
        internal static Task DeleteResourcesAsync(this IDurableOrchestrationContext functionContext, bool wait, params string[] resourceIds)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            resourceIds = resourceIds
                .Where(resourceId => !string.IsNullOrEmpty(resourceId))
                .ToArray();

            if (resourceIds.Any())
            {
                if (wait)
                    return functionContext.CallSubOrchestratorAsync(nameof(ResourceDeleteOrchestration), resourceIds.ToHashSet(StringComparer.OrdinalIgnoreCase));

                functionContext.StartNewOrchestration(nameof(ResourceDeleteOrchestration), resourceIds.ToHashSet(StringComparer.OrdinalIgnoreCase));
            }

            return Task.CompletedTask;
        }
    }
}
