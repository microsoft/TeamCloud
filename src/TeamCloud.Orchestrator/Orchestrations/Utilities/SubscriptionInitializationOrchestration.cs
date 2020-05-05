/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TeamCloud.Azure.Resources;
using TeamCloud.Orchestration.Deployment;
using TeamCloud.Orchestrator.Activities;
using TeamCloud.Orchestrator.Entities;
using TeamCloud.Orchestrator.Templates;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities
{
    public static class SubscriptionInitializationOrchestration
    {
        private static readonly Version TargetSubscriptionVersion = new InitializeSubscriptionTemplate().TemplateVersion;

        [FunctionName(nameof(SubscriptionInitializationOrchestration))]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var functionLog = functionContext.CreateReplaySafeLogger(log ?? NullLogger.Instance);

            var subscriptionId = functionContext.GetInput<Guid>();
            var resourceId = new AzureResourceIdentifier(subscriptionId);

            using (await functionContext.LockAzureResourceAsync(resourceId).ConfigureAwait(true))
            {
                var currentSubscriptionVersion = await functionContext
                    .GetSubscriptionVersionAsync(subscriptionId)
                    .ConfigureAwait(true);

                if (currentSubscriptionVersion != TargetSubscriptionVersion)
                {
                    // we have to offload the subscription initialization deployment to
                    // an independant orchestration (StartDeploymentAsync) as we initiate
                    // the deployment inside a critical section. this doesn't allow us
                    // to run the deploy as a nested orchestration (CallDeploymentAsync).

                    var deploymentOutputEventName = subscriptionId.ToString();

                    _ = await functionContext
                        .StartDeploymentAsync(nameof(ProjectSubscriptonInitializeActivity), subscriptionId, deploymentOutputEventName)
                        .ConfigureAwait(true);

                    await functionContext
                        .WaitForExternalEvent(deploymentOutputEventName, TimeSpan.FromMinutes(30))
                        .ConfigureAwait(true);

                    _ = await functionContext
                        .SetSubscriptionVersionAsync(subscriptionId, TargetSubscriptionVersion)
                        .ConfigureAwait(true);
                }
            }
        }
    }
}
