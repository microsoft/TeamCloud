/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
                var locks = await azure.ManagementLocks.ListByResourceGroupAsync(azureResourceGroup.ResourceGroupName, true).ConfigureAwait(false);
                var ids = locks.Select(s => s.Id).ToArray();
                if (ids.Length > 0)
                {
                    await azure.ManagementLocks.DeleteByIdsAsync(ids).ConfigureAwait(false);
                    // Wait one minute after deleting locks to ensure that Azure has updated itself before trying to delete the RG
                    await Task.Delay(60 * 1000).ConfigureAwait(false);
                }
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
    }
}