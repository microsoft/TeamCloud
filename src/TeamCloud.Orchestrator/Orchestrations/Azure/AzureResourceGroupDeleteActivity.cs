/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Rest.Azure;
using TeamCloud.Azure;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Orchestrations.Azure
{
    public class AzureResourceGroupDeleteActivity
    {
        private readonly IAzureSessionService azureSessionService;

        public AzureResourceGroupDeleteActivity(IAzureSessionService azure)
        {
            this.azureSessionService = azure ?? throw new ArgumentNullException(nameof(azure));
        }

        [FunctionName(nameof(AzureResourceGroupDeleteActivity))]
        public async Task RunActivity(
            [ActivityTrigger] AzureResourceGroup azureResourceGroup,
            ILogger log)
        {
            if (azureResourceGroup == null)
                throw new ArgumentNullException(nameof(azureResourceGroup));

            try
            {
                var azure = azureSessionService.CreateSession(azureResourceGroup.SubscriptionId);
                await DeleteResourceGroupLocksAsync(azureResourceGroup, azure).ConfigureAwait(false);
                await azure.ResourceGroups
                    .DeleteByNameAsync(azureResourceGroup.ResourceGroupName)
                    .ConfigureAwait(false);
            }
            catch (CloudException ex) when (ex.Body.Code.Equals("ResourceGroupNotFound", StringComparison.InvariantCultureIgnoreCase)) 
            {
                log.LogInformation($"Resource group '{azureResourceGroup.ResourceGroupName}' was not found in Azure, so nothing to delete.");
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Failed to delete resource group '{azureResourceGroup.ResourceGroupName}' in Azure.");
                throw;
            }
        }

        private async Task DeleteResourceGroupLocksAsync(AzureResourceGroup azureResourceGroup, IAzure azure)
        {
            string[] ids = await GetResourceGroupLockIdsAsync(azureResourceGroup, azure).ConfigureAwait(false);

            if (ids.Length > 0)
            {
                int counter = 1;
                do
                {
                    if (counter >= 10)
                        throw new Exception($"10 attempts were made to try and delete locks on Resource Group '{azureResourceGroup.ResourceGroupName}' and the locks still remain.  Resource group cannot be deleted right now.");
                    counter++;

                    // Delete locks within the RG
                    await azure.ManagementLocks.DeleteByIdsAsync(ids).ConfigureAwait(false);

                    // Check again to make sure there are no locks in the RG
                    ids = await GetResourceGroupLockIdsAsync(azureResourceGroup, azure).ConfigureAwait(false);
                }
                while (ids.Length != 0);
            }
        }

        private async Task<string[]> GetResourceGroupLockIdsAsync(AzureResourceGroup azureResourceGroup, IAzure azure)
        {
            var locks = await azure.ManagementLocks.ListByResourceGroupAsync(azureResourceGroup.ResourceGroupName, true).ConfigureAwait(false);
            var ids = locks.Select(s => s.Id).ToArray();
            return ids;
        }
    }
}
