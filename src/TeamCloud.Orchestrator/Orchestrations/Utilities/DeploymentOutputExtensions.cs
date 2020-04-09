/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities
{
    public static class DeploymentOutputExtensions
    {
        internal static Task<IReadOnlyDictionary<string, object>> GetDeploymentOutputAsync(this IDurableOrchestrationContext functionContext, string resourceId)
            => functionContext.CallSubOrchestratorAsync<IReadOnlyDictionary<string, object>>(nameof(DeploymentOutputOrchestration), resourceId);
    }
}
