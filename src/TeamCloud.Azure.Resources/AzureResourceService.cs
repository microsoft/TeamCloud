/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Models;

namespace TeamCloud.Azure.Resources
{

    public class AzureResourceService : IAzureResourceService
    {
        public AzureResourceService(IAzureSessionService azureSessionService)
        {
            AzureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
        }

        public IAzureSessionService AzureSessionService { get; }

        public Task<AzureSubscription> GetSubscriptionAsync(Guid subscriptionId, bool throwIfNotExists = false)
            => AzureResource.InitializeAsync(new AzureSubscription(subscriptionId), this, throwIfNotExists);

        public Task<AzureResourceGroup> GetResourceGroupAsync(Guid subscriptionId, string resourceGroupName, bool throwIfNotExists = false)
            => AzureResource.InitializeAsync(new AzureResourceGroup(subscriptionId, resourceGroupName), this, throwIfNotExists);

        public Task<AzureResource> GetResourceAsync(string resourceId, bool throwIfNotExists = false)
            => AzureResource.InitializeAsync(new AzureResource(resourceId), this, throwIfNotExists);

        public Task<TAzureResource> GetResourceAsync<TAzureResource>(string resourceId, bool throwIfNotExists = false)
            where TAzureResource : AzureResource
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var resource = Activator.CreateInstance(typeof(TAzureResource), bindingFlags, null, new object[] { resourceId }, CultureInfo.InvariantCulture) as TAzureResource;

            return AzureResource.InitializeAsync(resource, this, throwIfNotExists);
        }

        private async Task<ProviderInner> GetProviderAsync(Guid subscriptionId, string resourceNamespace)
        {
            if (resourceNamespace is null)
                throw new ArgumentNullException(nameof(resourceNamespace));

            using var resourceManagementClient = await AzureSessionService
                .CreateClientAsync<ResourceManagementClient>(subscriptionId: subscriptionId)
                .ConfigureAwait(false);

            return await resourceManagementClient.Providers
                .GetAsync(resourceNamespace)
                .ConfigureAwait(false);
        }

        public async Task RegisterProviderAsync(Guid subscriptionId, string resourceNamespace)
        {
            if (resourceNamespace is null)
                throw new ArgumentNullException(nameof(resourceNamespace));

            using var resourceManagementClient = await AzureSessionService
                .CreateClientAsync<ResourceManagementClient>(subscriptionId: subscriptionId)
                .ConfigureAwait(false);

            var provider = await resourceManagementClient.Providers
                .GetAsync(resourceNamespace)
                .ConfigureAwait(false);

            if (provider.RegistrationState.Equals("NotRegistered", StringComparison.OrdinalIgnoreCase)
                && provider.RegistrationPolicy.Equals("RegistrationRequired", StringComparison.OrdinalIgnoreCase))
            {
                await resourceManagementClient.Providers
                    .RegisterAsync(provider.NamespaceProperty)
                    .ConfigureAwait(false);
            }
        }

        public Task RegisterProvidersAsync(Guid subscriptionId, IEnumerable<string> resourceNamespaces)
            => Task.WhenAll(resourceNamespaces.Distinct().Select(n => RegisterProviderAsync(subscriptionId, n)));

        public async Task<IEnumerable<string>> GetApiVersionsAsync(Guid subscriptionId, string resourceNamespace, string resourceType, bool includePreviewVersions = false)
        {
            if (resourceNamespace is null)
                throw new ArgumentNullException(nameof(resourceNamespace));

            if (resourceType is null)
                throw new ArgumentNullException(nameof(resourceType));

            var provider = await GetProviderAsync(subscriptionId, resourceNamespace)
                .ConfigureAwait(false);

            var resourceTypeMatch = provider.ResourceTypes
                .FirstOrDefault(t => t.ResourceType.Equals(resourceType, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(resourceTypeMatch?.ResourceType))
                throw new ArgumentOutOfRangeException(nameof(resourceType));

            return includePreviewVersions ? resourceTypeMatch.ApiVersions.Where(v => !v.EndsWith("-preview", StringComparison.OrdinalIgnoreCase)) : resourceTypeMatch.ApiVersions;
        }
    }
}
