/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure.Resources;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Orchestrations.Utilities.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities
{
    public static class DeploymentExtensions
    {
        internal static Task ResetResourceGroupAsync(this IDurableOrchestrationContext functionContext, string resourceGroupId)
        {
            if (string.IsNullOrWhiteSpace(resourceGroupId))
                throw new ArgumentException($"Argument '{nameof(resourceGroupId)}' must not NULL or WHITESPACE", nameof(resourceGroupId));

            if (AzureResourceIdentifier.TryParse(resourceGroupId, out var resourceGroupIdentifier))
            {
                if (string.IsNullOrEmpty(resourceGroupIdentifier.ResourceGroup))
                    throw new ArgumentException($"Argument '{nameof(resourceGroupId)}' must contain a resource group name", nameof(resourceGroupId));

                return functionContext.GetDeploymentOutputAsync(nameof(ResourceGroupResetActivity), resourceGroupIdentifier.ToString(AzureResourceSegment.ResourceGroup));
            }

            throw new ArgumentException($"Invalid resource group Id: {resourceGroupId}", nameof(resourceGroupId));
        }


        internal static Task<IReadOnlyDictionary<string, object>> GetDeploymentOutputAsync(this IDurableOrchestrationContext functionContext, string deploymentResourceId)
            => functionContext.CallSubOrchestratorWithRetryAsync<IReadOnlyDictionary<string, object>>(nameof(DeploymentOrchestration), (default(string), default(object), deploymentResourceId));

        internal static Task<IReadOnlyDictionary<string, object>> GetDeploymentOutputAsync(this IDurableOrchestrationContext functionContext, string deploymentActivityName, object deploymentActivityInput = default)
            => functionContext.CallSubOrchestratorWithRetryAsync<IReadOnlyDictionary<string, object>>(nameof(DeploymentOrchestration), (deploymentActivityName, deploymentActivityInput, default(string)));
    }
}
