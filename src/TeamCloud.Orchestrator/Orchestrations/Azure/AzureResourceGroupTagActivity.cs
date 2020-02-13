/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Resources;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Orchestrations.Azure
{
    public class AzureResourceGroupTagActivity
    {
        private readonly IAzureResourceService azureResourceService;

        public AzureResourceGroupTagActivity(IAzureResourceService azureResourceService)
        {
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(AzureResourceGroupTagActivity))]
        public async Task RunActivity(
            [ActivityTrigger] Project project,
            ILogger log)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            var resourceGroup = await azureResourceService
                .GetResourceGroupAsync(project.ResourceGroup.SubscriptionId, project.ResourceGroup.ResourceGroupName, throwIfNotExists: true)
                .ConfigureAwait(false);

            await resourceGroup
                .SetTagsAsync(project.Tags)
                .ConfigureAwait(false);
        }
    }
}
