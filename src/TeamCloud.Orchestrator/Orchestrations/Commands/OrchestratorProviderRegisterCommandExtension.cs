/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Orchestrations.Commands
{
    internal static class OrchestratorProviderRegisterCommandExtension
    {
        public static Task RegisterProviderAsync(this IDurableOrchestrationContext functionContext, Provider provider = null, bool wait = true)
        {
            if (wait)
            {
                // if the caller request to wait for the provider registration
                // we will kick off the corresponding orchestration as a sub 
                // orchestration instead of completely new one

                return functionContext
                    .CallSubOrchestratorWithRetryAsync(nameof(OrchestratorProviderRegisterCommandOrchestration), (provider, default(ProviderRegisterCommand)));
            }

            functionContext
                .StartNewOrchestration(nameof(OrchestratorProviderRegisterCommandOrchestration), (provider, default(ProviderRegisterCommand)));

            return Task
                .CompletedTask;
        }
    }
}
