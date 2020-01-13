/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Azure
{
    public interface IAzureSessionOptions
    {
        string TenantId { get; }

        string ClientId { get; }

        string ClientSecret { get; }
    }
}
