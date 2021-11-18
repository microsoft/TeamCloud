/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Resources;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Command.Activities.Components
{
    public sealed class ComponentResourcesActivity
    {
        private readonly IAzureResourceService azureResourceService;

        public ComponentResourcesActivity(IAzureResourceService azureResourceService)
        {
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ComponentResourcesActivity))]
        [RetryOptions(3)]
        public async Task<string[]> Run(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var component = context.GetInput<Input>().Component;
            IAsyncEnumerable<AzureResource> resources = null;

            if (AzureResourceIdentifier.TryParse(component.ResourceId, out var componentResourceId))
            {
                if (string.IsNullOrEmpty(componentResourceId.ResourceGroup))
                {
                    var subscription = await azureResourceService
                        .GetSubscriptionAsync(componentResourceId.SubscriptionId)
                        .ConfigureAwait(false);

                    if (subscription is not null)
                    {
                        resources = subscription.GetResourceGroupsAsync()
                            .SelectMany(rg => rg.GetResourcesAsync());
                    }
                }
                else
                {
                    var resourceGroup = await azureResourceService
                        .GetResourceGroupAsync(componentResourceId.SubscriptionId, componentResourceId.ResourceGroup)
                        .ConfigureAwait(false);

                    if (resourceGroup is not null)
                    {
                        resources = resourceGroup.GetResourcesAsync();
                    }
                }
            }

            if (resources is null)
                return Array.Empty<string>();

            return await resources
                .Select(r => r.ResourceId.ToString())
                .ToArrayAsync()
                .ConfigureAwait(false);
        }

        internal struct Input
        {
            public Component Component { get; set; }
        }
    }
}
