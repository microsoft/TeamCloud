/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Azure;

public interface IAzureIdentity
{
    string ClientId { get; }

    string ObjectId { get; }

    string TenantId { get; }
}

public class AzureIdentity : IAzureIdentity
{
    public string ClientId { get; internal set; }

    public string ObjectId { get; internal set; }

    public string TenantId { get; internal set; }
}
