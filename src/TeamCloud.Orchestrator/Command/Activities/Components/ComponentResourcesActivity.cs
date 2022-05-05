/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using Azure.ResourceManager.Resources;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Command.Activities.Components;

public sealed class ComponentResourcesActivity
{
    private readonly IAzureService azureService;

    public ComponentResourcesActivity(IAzureService azureService)
    {
        this.azureService = azureService ?? throw new ArgumentNullException(nameof(azureService));
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

        IAsyncEnumerable<GenericResource> resources = null;

        if (!string.IsNullOrEmpty(component.ResourceId))
        {
            var resourceId = new ResourceIdentifier(component.ResourceId);

            if (string.IsNullOrEmpty(resourceId.ResourceGroupName))
            {
                var subscription = await azureService
                    .GetSubscriptionAsync(resourceId.SubscriptionId)
                    .ConfigureAwait(false);

                if (subscription is not null)
                {
                    resources = subscription.GetGenericResourcesAsync();
                }
            }
            else
            {
                var resourceGroup = await azureService
                    .GetResourceGroupAsync(resourceId.SubscriptionId, resourceId.ResourceGroupName)
                    .ConfigureAwait(false);

                if (resourceGroup is not null)
                {
                    resources = resourceGroup.GetGenericResourcesAsync();
                }
            }
        }

        if (resources is null)
            return Array.Empty<string>();

        return await resources
            .Select(r => r.Id.ToString())
            .ToArrayAsync()
            .ConfigureAwait(false);
    }

    internal struct Input
    {
        public Component Component { get; set; }
    }
}
