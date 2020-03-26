/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamCloud.Azure.Resources
{
    public sealed class AzureResourceGroup : AzureResource
    {
        private static string GetResourceId(Guid subscriptionId, string resourceGroupName)
            => new AzureResourceIdentifier(subscriptionId, resourceGroupName).ToString();

        internal AzureResourceGroup(Guid subscriptionId, string resourceGroupName)
            : base(GetResourceId(subscriptionId, resourceGroupName))
        { }

        public AzureResourceGroup(Guid subscriptionId, string resourceGroupName, IAzureResourceService azureResourceService)
            : base(GetResourceId(subscriptionId, resourceGroupName), azureResourceService)
        { }

        public override Task<bool> ExistsAsync()
        {
            var session = AzureResourceService.AzureSessionService
                .CreateSession(this.ResourceId.SubscriptionId);

            return session.ResourceGroups
                .ContainAsync(this.ResourceId.ResourceGroup);
        }

        public override async Task DeleteAsync(bool deleteLocks = false)
        {
            if (deleteLocks)
            {
                await DeleteLocksAsync(true)
                    .ConfigureAwait(false);
            }

            await base.DeleteAsync(false)
                .ConfigureAwait(false);
        }

        public override async Task DeleteLocksAsync(bool waitForDeletion = false)
        {
            var locks = await GetLocksInternalAsync()
                .ConfigureAwait(false);

            if (locks.Any())
            {
                var session = AzureResourceService.AzureSessionService
                    .CreateSession(this.ResourceId.SubscriptionId);

                await session.ManagementLocks
                    .DeleteByIdsAsync(locks.ToArray())
                    .ConfigureAwait(false);

                if (waitForDeletion)
                {
                    var timeoutDuration = TimeSpan.FromMinutes(5);
                    var timeout = DateTime.UtcNow.Add(timeoutDuration);

                    while (DateTime.UtcNow < timeout && locks.Any())
                    {
                        await Task.Delay(5000)
                            .ConfigureAwait(false);

                        locks = await GetLocksInternalAsync()
                            .ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task<IEnumerable<string>> GetLocksInternalAsync()
        {
            var session = AzureResourceService.AzureSessionService
                .CreateSession(this.ResourceId.SubscriptionId);

            var page = await session.ManagementLocks
                .ListByResourceGroupAsync(ResourceId.ResourceGroup, loadAllPages: true)
                .ConfigureAwait(false);

            return page.Select(lck => lck.Id);
        }
    }
}