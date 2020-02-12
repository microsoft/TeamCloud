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
using Microsoft.Rest.Azure;
using TeamCloud.Azure;
using TeamCloud.Azure.Deployment;
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Templates;

namespace TeamCloud.Orchestrator.Orchestrations.Azure
{
    public class AzureResourceGroupDeleteActivity
    {
        private readonly IAzureDeploymentService azureDeploymentService;
        private readonly IAzureSessionService azureSessionService;

        public AzureResourceGroupDeleteActivity(IAzureDeploymentService azureDeploymentService, IAzureSessionService azure)
        {
            this.azureDeploymentService = azureDeploymentService ?? throw new ArgumentNullException(nameof(azureDeploymentService));
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
                await CleanupResourceGroupAsync(azureResourceGroup)
                    .ConfigureAwait(false);

                await DeleteResourceGroupAsync(azureResourceGroup)
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

        private async Task CleanupResourceGroupAsync(AzureResourceGroup azureResourceGroup)
        {
            var template = new CleanupProjectTemplate();

            var deployment = await azureDeploymentService
                .DeployTemplateAsync(template, azureResourceGroup.SubscriptionId, azureResourceGroup.ResourceGroupName, completeMode: true)
                .ConfigureAwait(false);

            _ = await deployment
                .WaitAsync(throwOnError: true)
                .ConfigureAwait(false);
        }

        private async Task DeleteResourceGroupAsync(AzureResourceGroup azureResourceGroup)
        {
            var session = azureSessionService.CreateSession(azureResourceGroup.SubscriptionId);

            var mgmtLocks = await GetManagementLocksAsync()
                .ConfigureAwait(false);

            if (mgmtLocks.Any())
            {
                await session.ManagementLocks
                    .DeleteByIdsAsync(mgmtLocks.ToArray())
                    .ConfigureAwait(false);

                var timeoutDuration = TimeSpan.FromMinutes(5);
                var timeout = DateTime.UtcNow.Add(timeoutDuration);

                while (DateTime.UtcNow < timeout && mgmtLocks.Any())
                {
                    await Task.Delay(5000).ConfigureAwait(false);

                    mgmtLocks = await GetManagementLocksAsync()
                        .ConfigureAwait(false);
                }
            }

            await session.ResourceGroups
                .DeleteByNameAsync(azureResourceGroup.ResourceGroupName)
                .ConfigureAwait(false);

            async Task<IEnumerable<string>> GetManagementLocksAsync()
            {
                var locks = await session.ManagementLocks
                    .ListByResourceGroupAsync(azureResourceGroup.ResourceGroupName, loadAllPages: true)
                    .ConfigureAwait(false);

                return locks.Select(lck => lck.Id);
            }
        }
    }
}
