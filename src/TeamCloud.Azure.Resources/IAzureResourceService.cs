/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TeamCloud.Azure.Resources;

public interface IAzureResourceService
{
    IAzureSessionService AzureSessionService { get; }

    Task<AzureSubscription> GetSubscriptionAsync(Guid subscriptionId, bool throwIfNotExists = false);

    Task<AzureResourceGroup> GetResourceGroupAsync(Guid subscriptionId, string resourceGroupName, bool throwIfNotExists = false);

    async Task<bool> ExistsResourceAsync(string resourceId)
        => (await GetResourceAsync(resourceId).ConfigureAwait(false)) is not null;

    Task<AzureResource> GetResourceAsync(string resourceId, bool throwIfNotExists = false);

    Task<TAzureResource> GetResourceAsync<TAzureResource>(string resourceId, bool throwIfNotExists = false)
        where TAzureResource : AzureResource;

    // Task RegisterProviderAsync(Guid subscriptionId, string resourceNamespace);

    // Task RegisterProvidersAsync(Guid subscriptionId, IEnumerable<string> resourceNamespaces);

    Task<IEnumerable<string>> GetApiVersionsAsync(Guid subscriptionId, string resourceNamespace, string resourceType, bool includePreviewVersions = false);
}
