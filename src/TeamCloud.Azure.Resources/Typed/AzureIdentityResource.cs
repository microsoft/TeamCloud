/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Msi.Fluent;
using TeamCloud.Azure.Resources.Utilities;

namespace TeamCloud.Azure.Resources.Typed;

public sealed class AzureIdentityResource : AzureTypedResource, IAzureIdentity
{
    private readonly AsyncLazy<IIdentity> identityInstance;

    public AzureIdentityResource(string resourceId) : base("Microsoft.ManagedIdentity/userAssignedIdentities", resourceId)
    {
        identityInstance = new AsyncLazy<IIdentity>(GetIdentityAsync);
    }

    private async Task<IIdentity> GetIdentityAsync()
    {
        var session = await AzureResourceService.AzureSessionService
            .CreateSessionAsync(ResourceId.SubscriptionId)
            .ConfigureAwait(false);

        return await session.Identities
            .GetByIdAsync(ResourceId.ToString())
            .ConfigureAwait(false);
    }

    // public string ClientId
    //     => identityInstance.Value.Result.ClientId;

    // public string TenantId
    //     => identityInstance.Value.Result.TenantId;

    public string PrincipalId
        => identityInstance.Value.Result.PrincipalId;

    // public string ClientSecretUrl
    //     => identityInstance.Value.Result.ClientSecretUrl;

    public override async IAsyncEnumerable<IAzureIdentity> GetIdentitiesAsync()
    {
        _ = await identityInstance
            .ConfigureAwait(false);

        yield return this;
    }
}
