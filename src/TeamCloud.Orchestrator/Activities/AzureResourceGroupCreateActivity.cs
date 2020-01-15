/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Activities
{
    public class AzureResourceGroupCreateActivity
    {
        private readonly IAzureSessionFactory azureSessionFactory;

        public AzureResourceGroupCreateActivity(IAzureSessionFactory azureSessionFactory)
        {
            this.azureSessionFactory = azureSessionFactory ?? throw new ArgumentNullException(nameof(azureSessionFactory));
        }

        [FunctionName(nameof(AzureResourceGroupCreateActivity))]
        public async Task<string> RunActivity(
            [ActivityTrigger] AzureResourceGroup azureResourceGroup)
        {
            if (azureResourceGroup == null)
                throw new ArgumentNullException(nameof(azureResourceGroup));

            // Create instance to Azure instance
            var azureSession = azureSessionFactory.CreateSession(Guid.Parse(azureResourceGroup.SubscriptionId));

            // TODO Should we retrieve existing group if the name is already used?

            // Check if group already exists
            if (await azureSession.ResourceGroups.ContainAsync(azureResourceGroup.ResourceGroupName).ConfigureAwait(false) == false)
            {
                // Create new group
                var newGroup = await azureSession.ResourceGroups
                    .Define(azureResourceGroup.ResourceGroupName)
                    .WithRegion(azureResourceGroup.Region)
                    .CreateAsync()
                    .ConfigureAwait(false);

                return newGroup.Id;
            }
            else
            {
                // Retrieve existing group
                var existingGroup = await azureSession.ResourceGroups
                    .GetByNameAsync(azureResourceGroup.ResourceGroupName)
                    .ConfigureAwait(false);

                return existingGroup.Id;
            }
        }
    }
}