/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Azure;

public interface IAzureSessionIdentity
{
    Guid ClientId { get; }

    Guid ObjectId { get; }

    Guid TenantId { get; }
}

public class AzureSessionIdentity : IAzureSessionIdentity
{
    public Guid ClientId { get; internal set; }

    public Guid ObjectId { get; internal set; }

    public Guid TenantId { get; internal set; }
}
