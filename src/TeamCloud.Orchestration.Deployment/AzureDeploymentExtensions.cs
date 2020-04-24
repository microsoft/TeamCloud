/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace TeamCloud.Orchestration.Deployment
{
    public static class AzureDeploymentExtensions
    {
        public static Task<IReadOnlyDictionary<string, object>> GetDeploymentOutputAsync(this IDurableOrchestrationContext functionContext, string deploymentResourceId)
        {
            if (functionContext is null)
                throw new System.ArgumentNullException(nameof(functionContext));

            if (string.IsNullOrEmpty(deploymentResourceId))
                throw new System.ArgumentException("message", nameof(deploymentResourceId));

            return functionContext
                .CallSubOrchestratorWithRetryAsync<IReadOnlyDictionary<string, object>>(nameof(AzureDeploymentOrchestration), (default(string), default(object), deploymentResourceId, false));
        }


        public static Task<IReadOnlyDictionary<string, object>> GetDeploymentOutputAsync(this IDurableOrchestrationContext functionContext, string deploymentActivityName, object deploymentActivityInput = default)
        {
            if (functionContext is null)
                throw new System.ArgumentNullException(nameof(functionContext));

            if (string.IsNullOrEmpty(deploymentActivityName))
                throw new System.ArgumentException("message", nameof(deploymentActivityName));

            return functionContext
                .CallSubOrchestratorWithRetryAsync<IReadOnlyDictionary<string, object>>(nameof(AzureDeploymentOrchestration), (deploymentActivityName, deploymentActivityInput, default(string), false));
        }
    }
}
