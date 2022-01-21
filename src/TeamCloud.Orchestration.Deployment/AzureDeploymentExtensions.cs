/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Orchestration.Deployment.Activities;

namespace TeamCloud.Orchestration.Deployment;

public static class AzureDeploymentExtensions
{
    public static Task<IReadOnlyDictionary<string, object>> WaitForDeploymentOutput(this IDurableOrchestrationContext orchestrationContext, string deploymentOutputEventName, TimeSpan timeout)
    {
        if (orchestrationContext is null)
            throw new ArgumentNullException(nameof(orchestrationContext));

        if (string.IsNullOrEmpty(deploymentOutputEventName))
            throw new ArgumentException($"'{nameof(deploymentOutputEventName)}' cannot be null or empty", nameof(deploymentOutputEventName));

        return orchestrationContext.WaitForExternalEvent<IReadOnlyDictionary<string, object>>(deploymentOutputEventName, timeout);
    }

    public static Task<IReadOnlyDictionary<string, object>> GetDeploymentOutputAsync(this IDurableOrchestrationContext orchestrationContext, string deploymentResourceId)
    {
        if (orchestrationContext is null)
            throw new System.ArgumentNullException(nameof(orchestrationContext));

        if (string.IsNullOrEmpty(deploymentResourceId))
            throw new System.ArgumentException("message", nameof(deploymentResourceId));

        return orchestrationContext
            .CallActivityWithRetryAsync<IReadOnlyDictionary<string, object>>(nameof(AzureDeploymentOutputActivity), deploymentResourceId);
    }

    public static Task<IReadOnlyDictionary<string, object>> CallDeploymentAsync(this IDurableOrchestrationContext orchestrationContext, string deploymentActivityName, object deploymentActivityInput = default)
    {
        if (orchestrationContext is null)
            throw new System.ArgumentNullException(nameof(orchestrationContext));

        if (string.IsNullOrEmpty(deploymentActivityName))
            throw new System.ArgumentException("message", nameof(deploymentActivityName));

        return orchestrationContext
            .CallSubOrchestratorWithRetryAsync<IReadOnlyDictionary<string, object>>(nameof(AzureDeploymentOrchestration), new AzureDeploymentOrchestration.Input()
            {
                DeploymentOwnerInstanceId = orchestrationContext.InstanceId,
                DeploymentActivityName = deploymentActivityName,
                DeploymentActivityInput = deploymentActivityInput
            });
    }

    public static Task<string> StartDeploymentAsync(this IDurableOrchestrationContext orchestrationContext, string deploymentActivityName, object deploymentActivityInput = default, string deploymentOutputEventName = default)
    {
        if (orchestrationContext is null)
            throw new System.ArgumentNullException(nameof(orchestrationContext));

        if (string.IsNullOrEmpty(deploymentActivityName))
            throw new System.ArgumentException("message", nameof(deploymentActivityName));

        var instanceId = orchestrationContext
            .StartNewOrchestration(nameof(AzureDeploymentOrchestration), new AzureDeploymentOrchestration.Input()
            {
                DeploymentOwnerInstanceId = orchestrationContext.InstanceId,
                DeploymentActivityName = deploymentActivityName,
                DeploymentActivityInput = deploymentActivityInput,
                DeploymentOutputEventName = deploymentOutputEventName
            });

        return Task.FromResult(instanceId);
    }
}
