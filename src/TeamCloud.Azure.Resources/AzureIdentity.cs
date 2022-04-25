/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Azure.Resources;

public interface IAzureIdentity
{
    // string ClientId { get; }

    // string TenantId { get; }

    string PrincipalId { get; }

    // string ClientSecretUrl { get; }
}

public sealed class AzureIdentity : IAzureIdentity
{
    // public string ClientId { get; set; }

    // public string TenantId { get; set; }

    public string PrincipalId { get; set; }

    // public string ClientSecretUrl { get; set; }
}
