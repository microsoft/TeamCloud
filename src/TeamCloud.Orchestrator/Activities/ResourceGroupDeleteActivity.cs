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
using TeamCloud.Azure.Resources;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Activities
{
    public class ResourceGroupDeleteActivity
    {
        private readonly IAzureResourceService azureResourceService;

        public ResourceGroupDeleteActivity(IAzureResourceService azureResourceService)
        {
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ResourceGroupDeleteActivity))]
        [RetryOptions(3)]
        public async Task RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var resourceId = functionContext.GetInput<string>();

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

                    await (resourceGroup?.DeleteAsync(true) ?? Task.CompletedTask)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"Activity '{nameof(ResourceGroupDeleteActivity)} failed: {exc.Message}");

                throw exc.AsSerializable();
            }
        }
    }
}
