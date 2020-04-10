/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Deployment;
using TeamCloud.Azure.Resources;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Templates;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities.Activities
{
    public class ResetResourceGroupActivity
    {
        private readonly IAzureDeploymentService azureDeploymentService;

        public ResetResourceGroupActivity(IAzureDeploymentService azureDeploymentService)
        {
            this.azureDeploymentService = azureDeploymentService ?? throw new ArgumentNullException(nameof(azureDeploymentService));
        }

        [FunctionName(nameof(ResetResourceGroupActivity))]
        [RetryOptions(3)]
        public async Task<string> RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var resourceGroupId = functionContext.GetInput<string>();

            try
            {
                var resourceGroupIdentifier = AzureResourceIdentifier.Parse(resourceGroupId);

                if (string.IsNullOrEmpty(resourceGroupIdentifier.ResourceGroup))
                    throw new RetryCanceledException($"Resource id does not identify a resource group: {resourceGroupId}");

                var deployment = await azureDeploymentService
                    .DeployResourceGroupTemplateAsync(new ResetResourceGroupTemplate(), resourceGroupIdentifier.SubscriptionId, resourceGroupIdentifier.ResourceGroup, completeMode: true)
                    .ConfigureAwait(false);

                return deployment.ResourceId;
            }
            catch (Exception exc) when (!exc.IsSerializable(out var serializableException))
            {
                log.LogError(exc, $"Activity '{nameof(ResetResourceGroupActivity)} failed: {exc.Message}");

                throw serializableException;
            }
        }
    }
}
