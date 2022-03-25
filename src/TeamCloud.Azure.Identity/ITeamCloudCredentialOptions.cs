/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Azure.Identity;

public interface ITeamCloudCredentialOptions
{
    string ClientId { get; }

    string ClientSecret { get; }

    string TenantId { get; }
}
