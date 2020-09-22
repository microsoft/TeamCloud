/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
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
            [ActivityTrigger] IDurableActivityContext activityContext,
            ILogger log)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            var resourceId = activityContext.GetInput<string>();

            try
            {
                var resourceIdentifier = AzureResourceIdentifier.Parse(resourceId);

                if (string.IsNullOrEmpty(resourceIdentifier.ResourceGroup))
                {
                    throw new RetryCanceledException($"Resource id must include resource group information: {resourceId}");
                }
                else if (resourceIdentifier.ResourceTypes?.Any() ?? false)
                {
                    throw new RetryCanceledException($"Resource id must not include resource type information: {resourceId}");
                }
                else
                {
                    var resourceGroup = await azureResourceService
                        .GetResourceGroupAsync(resourceIdentifier.SubscriptionId, resourceIdentifier.ResourceGroup)
                        .ConfigureAwait(false);

                    if (resourceGroup is null)
                        return default;

                    var deployment = await azureDeploymentService
                        .DeployResourceGroupTemplateAsync(new ResetResourceGroupTemplate(), resourceIdentifier.SubscriptionId, resourceIdentifier.ResourceGroup, completeMode: true)
                        .ConfigureAwait(false);

                    return deployment.ResourceId;
                }
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"Activity '{nameof(ResourceGroupResetActivity)} failed: {exc.Message}");

                throw exc.AsSerializable();
            }
        }
    }
}
