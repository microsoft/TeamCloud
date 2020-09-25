/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Models;
using Microsoft.Rest.Azure.OData;

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

        public override async Task<bool> ExistsAsync()
        {
            var session = await AzureResourceService.AzureSessionService
                .CreateSessionAsync(this.ResourceId.SubscriptionId)
                .ConfigureAwait(false);

            return await session.ResourceGroups
                .ContainAsync(this.ResourceId.ResourceGroup)
                .ConfigureAwait(false);
        }

        public override async Task DeleteAsync(bool deleteLocks = false)
        {
            if (deleteLocks)
            {
                await DeleteLocksAsync(true)
                    .ConfigureAwait(false);
            }

            var timeoutDuration = TimeSpan.FromMinutes(1);
            var timeout = DateTime.UtcNow.Add(timeoutDuration);

            while (DateTime.UtcNow < timeout)
            {
                try
                {
                    await base
                        .DeleteAsync(false)
                        .ConfigureAwait(false);

                    break;
                }
                catch (FlurlHttpException exc) when (deleteLocks && exc.Call.HttpStatus == System.Net.HttpStatusCode.Conflict)
                {
                    // swallow exception take a rest and retry the delete call
                    // there is a chance that the in a "delete locks" scenario
                    // the delete operation is entirely done even if no lock
                    // is reported back to the caller

                    await Task
                        .Delay(1000)
                        .ConfigureAwait(false);
                }
            }
        }

        public override async Task DeleteLocksAsync(bool waitForDeletion = false)
        {
            var locks = await GetLocksInternalAsync()
                .ConfigureAwait(false);

            if (locks.Any())
            {
                var session = await AzureResourceService.AzureSessionService
                    .CreateSessionAsync(this.ResourceId.SubscriptionId)
                    .ConfigureAwait(false);

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

        public IAsyncEnumerable<AzureResource> GetResourcesAsync()
            => GetResourcesAsync(default);

        public IAsyncEnumerable<AzureResource> GetResourcesByTypeAsync(string resourceType)
        {
            if (string.IsNullOrWhiteSpace(resourceType))
                throw new ArgumentException($"Resource type must not NULL or WHITESPACE", nameof(resourceType));

            return GetResourcesAsync(new ODataQuery<GenericResourceFilter>(resourceFilter => resourceFilter.ResourceType == resourceType));
        }

        public IAsyncEnumerable<AzureResource> GetResourcesByTagAsync(string tagName, string tagValue = default)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                throw new ArgumentException("Tag name must not NULL or WHITESPACE", nameof(tagName));

            return string.IsNullOrWhiteSpace(tagValue)
                ? GetResourcesAsync(new ODataQuery<GenericResourceFilter>(resourceFilter => resourceFilter.Tagname == tagName))
                : GetResourcesAsync(new ODataQuery<GenericResourceFilter>(resourceFilter => resourceFilter.Tagname == tagName && resourceFilter.Tagvalue == tagValue));
        }


        private async IAsyncEnumerable<AzureResource> GetResourcesAsync(ODataQuery<GenericResourceFilter> resourceQuery = null)
        {
            using var resourceManagementClient = await AzureResourceService.AzureSessionService
                .CreateClientAsync<ResourceManagementClient>(subscriptionId: this.ResourceId.SubscriptionId)
                .ConfigureAwait(false);

            var page = await resourceManagementClient.Resources
                .ListByResourceGroupAsync(this.ResourceId.ResourceGroup, resourceQuery)
                .ConfigureAwait(false);

            var resources = page
                .AsContinuousCollectionAsync((nextPageLink) => resourceManagementClient.Resources.ListByResourceGroupNextAsync(nextPageLink))
                .ConfigureAwait(false);

            await foreach (var resource in resources)
            {
                var resourceGeneric = await AzureResourceService
                    .GetResourceAsync(resource.Id)
                    .ConfigureAwait(false);

                if (resourceGeneric != null)
                    yield return resourceGeneric;
            }
        }

        private async Task<IEnumerable<string>> GetLocksInternalAsync()
        {
            var session = await AzureResourceService.AzureSessionService
                .CreateSessionAsync(this.ResourceId.SubscriptionId)
                .ConfigureAwait(false);

            var page = await session.ManagementLocks
                .ListByResourceGroupAsync(ResourceId.ResourceGroup, loadAllPages: true)
                .ConfigureAwait(false);

            return page.Select(lck => lck.Id);
        }
    }
}