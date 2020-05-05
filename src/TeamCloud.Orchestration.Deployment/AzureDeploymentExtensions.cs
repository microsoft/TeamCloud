/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Orchestration.Deployment.Activities;

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
                .CallActivityWithRetryAsync<IReadOnlyDictionary<string, object>>(nameof(AzureDeploymentOutputActivity), deploymentResourceId);
        }

        public static Task<IReadOnlyDictionary<string, object>> CallDeploymentAsync(this IDurableOrchestrationContext functionContext, string deploymentActivityName, object deploymentActivityInput = default)
        {
            if (functionContext is null)
                throw new System.ArgumentNullException(nameof(functionContext));

            if (string.IsNullOrEmpty(deploymentActivityName))
                throw new System.ArgumentException("message", nameof(deploymentActivityName));

            return functionContext
                .CallSubOrchestratorWithRetryAsync<IReadOnlyDictionary<string, object>>(nameof(AzureDeploymentOrchestration), (functionContext.InstanceId, deploymentActivityName, deploymentActivityInput, default(string), default(string)));
        }

        public static Task<string> StartDeploymentAsync(this IDurableOrchestrationContext functionContext, string deploymentActivityName, object deploymentActivityInput = default, string deploymentOutputEventName = default)
        {
            if (functionContext is null)
                throw new System.ArgumentNullException(nameof(functionContext));

            if (string.IsNullOrEmpty(deploymentActivityName))
                throw new System.ArgumentException("message", nameof(deploymentActivityName));

            var instanceId = functionContext
                .StartNewOrchestration(nameof(AzureDeploymentOrchestration), (functionContext.InstanceId, deploymentActivityName, deploymentActivityInput, default(string), deploymentOutputEventName));

            return Task.FromResult(instanceId);
        }
    }
}
