/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Flurl.Http.Configuration;
using TeamCloud.Azure.ContainerInstance;
using TeamCloud.Azure.KeyVault;
using TeamCloud.Azure.Storage;
using IHttpClientFactory = Flurl.Http.Configuration.IHttpClientFactory;

namespace TeamCloud.Azure;

public interface IAzureService
{
    IStorageService Storage { get; }
    IKeyVaultService KeyVaults { get; }
    IContainerInstanceService ContainerInstances { get; }
    ArmEnvironment ArmEnvironment { get; }
    Task<string> GetTenantIdAsync(CancellationToken cancellationToken = default);
    SubscriptionResource GetSubscription(string subscriptionId);
    Task<string> AcquireTokenAsync(CancellationToken cancellationToken = default);
    Task<SubscriptionResource> GetSubscriptionAsync(string subscriptionId, bool throwIfNotExists = false, CancellationToken cancellationToken = default);
    Task<ResourceGroupResource> GetResourceGroupAsync(string subscriptionId, string resourceGroupName, bool throwIfNotExists = false, CancellationToken cancellationToken = default);
    Task DeleteResourceAsync(string resourceId, bool deleteLocks = false, CancellationToken cancellationToken = default);
    Task<GenericResource> GetUserAssignedIdentiyAsync(string subscriptionId, string resourceGroupName, string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetApiVersionsAsync(string subscriptionId, string resourceProviderNamespace, string resourceType, bool includePreviewVersions = false, CancellationToken cancellationToken = default);
}

public class AzureService : IAzureService
{
    private readonly IArmService arm;
    private readonly IStorageService storageService;
    private readonly IKeyVaultService keyVaultService;
    private readonly IContainerInstanceService containerInstanceService;
    private readonly IAzureSessionOptions azureSessionOptions;
    private readonly IHttpClientFactory httpClientFactory;

    public AzureService(
        IArmService arm,
        IStorageService storageService,
        IKeyVaultService keyVaultService,
        IContainerInstanceService containerInstanceService,
        IAzureSessionOptions azureSessionOptions = null,
        IHttpClientFactory httpClientFactory = null)
    {
        this.arm = arm ?? throw new ArgumentNullException(nameof(arm));
        this.storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        this.keyVaultService = keyVaultService ?? throw new ArgumentNullException(nameof(keyVaultService));
        this.containerInstanceService = containerInstanceService ?? throw new ArgumentNullException(nameof(containerInstanceService));

        this.azureSessionOptions = azureSessionOptions ?? AzureSessionOptions.Default;

        this.httpClientFactory = httpClientFactory ?? new DefaultHttpClientFactory();
    }

    public IStorageService Storage => storageService;
    public IKeyVaultService KeyVaults => keyVaultService;
    public IContainerInstanceService ContainerInstances => containerInstanceService;

    public ArmEnvironment ArmEnvironment => arm.ArmEnvironment;

    public Task<string> GetTenantIdAsync(CancellationToken cancellationToken = default) => arm.GetTenantIdAsync(cancellationToken);
    public Task<IAzureIdentity> GetIdentityAsync(CancellationToken cancellationToken = default) => arm.GetIdentityAsync(cancellationToken);
    public Task<string> AcquireTokenAsync(CancellationToken cancellationToken = default) => arm.AcquireTokenAsync(cancellationToken);


    public SubscriptionResource GetSubscription(string subscriptionId)
        => arm.GetArmClient(subscriptionId)
            .GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscriptionId));

    public async Task<SubscriptionResource> GetSubscriptionAsync(string subscriptionId, bool throwIfNotExists = false, CancellationToken cancellationToken = default)
    {
        var subscriptions = arm.GetArmClient().GetSubscriptions();

        if (!throwIfNotExists)
        {
            var exists = await subscriptions
                .ExistsAsync(subscriptionId, cancellationToken)
                .ConfigureAwait(false);

            if (!exists.Value)
            {
                return null;
            }
        }

        var response = await subscriptions
            .GetAsync(subscriptionId, cancellationToken)
            .ConfigureAwait(false);

        return response.Value;
    }

    private ResourceGroupCollection GetResourceGroups(string subscriptionId)
        => GetSubscription(subscriptionId)
            .GetResourceGroups();

    private ResourceGroupResource GetResourceGroup(string subscriptionId, string resourceGroupName)
        => arm.GetArmClient(subscriptionId)
            .GetResourceGroupResource(ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroupName));

    public async Task<ResourceGroupResource> GetResourceGroupAsync(string subscriptionId, string resourceGroupName, bool throwIfNotExists = false, CancellationToken cancellationToken = default)
    {
        var groups = GetResourceGroups(subscriptionId);

        if (!throwIfNotExists)
        {
            var exists = await groups
                .ExistsAsync(resourceGroupName, cancellationToken)
                .ConfigureAwait(false);

            if (!exists.Value)
            {
                return null;
            }
        }

        var response = await groups
            .GetAsync(resourceGroupName, cancellationToken)
            .ConfigureAwait(false);

        return response.Value;
    }

    public async Task<GenericResource> GetUserAssignedIdentiyAsync(string subscriptionId, string resourceGroupName, string name, CancellationToken cancellationToken = default)
    {
        var id = ResourceGroupResource
            .CreateResourceIdentifier(subscriptionId, resourceGroupName)
            .AppendProviderResource("Microsoft.ManagedIdentity", "userAssignedIdentities", name);

        var identity = await arm.GetArmClient(subscriptionId)
            .GetGenericResources()
            .GetAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return identity.Value;
    }


    public async Task DeleteResourceAsync(string resourceId, bool deleteLocks = false, CancellationToken cancellationToken = default)
    {
        var id = new ResourceIdentifier(resourceId);

        var resources = arm.GetArmClient(id.SubscriptionId)
            .GetGenericResources();

        var exists = await resources
            .ExistsAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (!exists.Value)
            return;

        var resource = await resources
            .GetAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (deleteLocks)
        {
            var locks = await resource.Value.GetManagementLocks()
                .GetAllAsync(cancellationToken: cancellationToken)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            await Task.WhenAll(locks.Select(l => l.DeleteAsync(WaitUntil.Completed, cancellationToken)))
                .ConfigureAwait(false);
        }

        await resource.Value
            .DeleteAsync(WaitUntil.Completed, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<ResourceProviderResource> GetProviderAsync(string subscriptionId, string resourceProviderNamespace, CancellationToken cancellationToken = default)
    {
        var response = await GetSubscription(subscriptionId)
            .GetResourceProviders()
            .GetAsync(resourceProviderNamespace, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return response.Value;
    }

    public async Task<IEnumerable<string>> GetApiVersionsAsync(string subscriptionId, string resourceProviderNamespace, string resourceType, bool includePreviewVersions = false, CancellationToken cancellationToken = default)
    {
        if (resourceProviderNamespace is null)
            throw new ArgumentNullException(nameof(resourceProviderNamespace));

        if (resourceType is null)
            throw new ArgumentNullException(nameof(resourceType));

        var provider = await GetProviderAsync(subscriptionId, resourceProviderNamespace, cancellationToken)
            .ConfigureAwait(false);

        var resourceTypeMatch = provider.Data.ResourceTypes
            .FirstOrDefault(t => t.ResourceType.Equals(resourceType, StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrEmpty(resourceTypeMatch?.ResourceType))
            throw new ArgumentOutOfRangeException(nameof(resourceType));

        return includePreviewVersions ? resourceTypeMatch.ApiVersions.Where(v => !v.EndsWith("-preview", StringComparison.OrdinalIgnoreCase)) : resourceTypeMatch.ApiVersions;
    }
}
