/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.ResourceManager.KeyVault;
using Azure.ResourceManager.KeyVault.Models;
using Azure.Security.KeyVault.Secrets;

namespace TeamCloud.Azure.KeyVault;

public interface IKeyVaultService
{
    public Task<VaultResource> GetKeyVaultAsync(string resourceId, CancellationToken cancellationToken = default);
    public Task SetAllSecretPermissionsAsync(string resourceId, string userId, CancellationToken cancellationToken = default);
    public Task<SecretClient> GetSecretClientAsync(string resourceId, bool ensureIdentityAccess = true, CancellationToken cancellationToken = default);
    IAsyncEnumerable<KeyValuePair<string, string>> GetSecretsAsync(string resourceId, bool ensureIdentityAccess = true, CancellationToken cancellationToken = default);
}

public class KeyVaultService : IKeyVaultService
{
    private readonly IArmService arm;
    public KeyVaultService(IArmService arm)
    {
        this.arm = arm ?? throw new ArgumentNullException(nameof(arm));
    }

    private readonly ConcurrentDictionary<string, SecretClient> secretClientMap = new(StringComparer.OrdinalIgnoreCase);


    private VaultResource GetKeyVault(string resourceId)
    {
        var id = new ResourceIdentifier(resourceId);

        return arm
            .GetArmClient(id.SubscriptionId)
            .GetVaultResource(id);
    }

    public async Task<VaultResource> GetKeyVaultAsync(string resourceId, CancellationToken cancellationToken = default)
    {
        var response = await GetKeyVault(resourceId)
            .GetAsync(cancellationToken)
            .ConfigureAwait(false);

        return response.Value;
    }

    public Task SetAllSecretPermissionsAsync(string resourceId, string userId, CancellationToken cancellationToken = default)
        => SetAllSecretPermissionsInternalAsync(null, resourceId, userId, cancellationToken);

    private async Task SetAllSecretPermissionsInternalAsync(VaultResource vault, string resourceId, string userId, CancellationToken cancellationToken = default)
    {
        vault ??= await GetKeyVaultAsync(resourceId, cancellationToken)
            .ConfigureAwait(false);

        if (!vault.Data.Properties.AccessPolicies.Any(ap => ap.ObjectId == userId))
        {
            var tenantId = vault.Data.Properties.TenantId;

            var accessPermissions = new AccessPermissions
            {
                // Keys = { new KeyPermissions("all") },
                Secrets = { new SecretPermissions("all") }
                // Certificates = { new CertificatePermissions("all") },
                // Storage = { new StoragePermissions("all") },
            };

            var accessPolicy = new AccessPolicyEntry(tenantId, userId, accessPermissions);

            var accessPolicyProperties = new VaultAccessPolicyProperties(new AccessPolicyEntry[] { accessPolicy });
            var accessPolicyParameters = new VaultAccessPolicyParameters(accessPolicyProperties);

            await vault
                .UpdateAccessPolicyAsync(AccessPolicyUpdateKind.Add, accessPolicyParameters, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public async Task<SecretClient> GetSecretClientAsync(string resourceId, bool ensureIdentityAccess = true, CancellationToken cancellationToken = default)
    {
        if (resourceId is null)
            throw new ArgumentNullException(nameof(resourceId));

        if (!secretClientMap.TryGetValue(resourceId, out var secretClient))
        {
            var vault = await GetKeyVaultAsync(resourceId, cancellationToken)
                .ConfigureAwait(false);

            var identity = await arm.GetIdentityAsync(cancellationToken)
                .ConfigureAwait(false);

            if (ensureIdentityAccess)
                await SetAllSecretPermissionsInternalAsync(vault, resourceId, identity.ObjectId, cancellationToken);

            var vaultUri = vault.Data.Properties.VaultUri;

            secretClient = new SecretClient(vaultUri, arm.GetTokenCredential());

            secretClientMap[resourceId] = secretClient;
        }

        return secretClient;
    }

    public async IAsyncEnumerable<KeyValuePair<string, string>> GetSecretsAsync(string resourceId, bool ensureIdentityAccess = true, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var client = await GetSecretClientAsync(resourceId, ensureIdentityAccess, cancellationToken)
            .ConfigureAwait(false);

        await foreach (var item in client.GetPropertiesOfSecretsAsync(cancellationToken))
        {
            var secret = await client.GetSecretAsync(item.Name, null, cancellationToken)
                .ConfigureAwait(false);

            yield return new KeyValuePair<string, string>(secret.Value.Name, secret.Value.Value);
        }
    }
}
