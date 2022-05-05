/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Linq;
using Azure.Core;
using Azure.Identity;

namespace TeamCloud.Azure.Identity;

public class TeamCloudCredential : ChainedTokenCredential
{
    private static IEnumerable<TokenCredential> GetTokenCredentialChain(ITeamCloudCredentialOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options?.TenantId) && !string.IsNullOrWhiteSpace(options?.ClientId) && !string.IsNullOrWhiteSpace(options?.ClientSecret))
            yield return new ClientSecretCredential(options.TenantId, options.ClientId, options.ClientSecret);

        yield return new DefaultAzureCredential();
    }

    public TeamCloudCredential(ITeamCloudCredentialOptions options = null) : base(GetTokenCredentialChain(options).ToArray())
    { }
}
