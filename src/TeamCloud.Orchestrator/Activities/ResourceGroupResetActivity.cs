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

namespace TeamCloud.Orchestrator.Activities
{
    public class ResourceGroupResetActivity
    {
        private readonly IAzureDeploymentService azureDeploymentService;
        private readonly IAzureResourceService azureResourceService;

        public ResourceGroupResetActivity(IAzureDeploymentService azureDeploymentService, IAzureResourceService azureResourceService)
        {
            this.azureDeploymentService = azureDeploymentService ?? throw new ArgumentNullException(nameof(azureDeploymentService));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ResourceGroupResetActivity))]
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

                var resourceGroup = await azureResourceService
                    .GetResourceGroupAsync(resourceGroupIdentifier.SubscriptionId, resourceGroupIdentifier.ResourceGroup)
                    .ConfigureAwait(false);

                if (resourceGroup is null)
                    return default;

                var deployment = await azureDeploymentService
                    .DeployResourceGroupTemplateAsync(new ResetResourceGroupTemplate(), resourceGroupIdentifier.SubscriptionId, resourceGroupIdentifier.ResourceGroup, completeMode: true)
                    .ConfigureAwait(false);

                return deployment.ResourceId;
            }
            catch (Exception exc) when (!exc.IsSerializable(out var serializableException))
            {
                log.LogError(exc, $"Activity '{nameof(ResourceGroupResetActivity)} failed: {exc.Message}");

                throw serializableException;
            }
        }
    }
}
