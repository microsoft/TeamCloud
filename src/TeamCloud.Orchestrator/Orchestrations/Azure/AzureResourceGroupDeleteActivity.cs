/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
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
            [ActivityTrigger] AzureResourceGroup azureResourceGroup)
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
            catch (CloudException ex) when (ex.Message.Contains("could not be found", StringComparison.InvariantCultureIgnoreCase)) 
            // TODO: Is this ok way to check for an exception? This only happens when the RG is already deleted and may not happen often. 
            // But ignoring this error could prevent the project in the DB from being deleted
            {
                Debug.WriteLine("Resource group is already deleted, ignore this exception: " + ex.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
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
