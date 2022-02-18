/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Rest;
using AZFluent = Microsoft.Azure.Management.Fluent;

namespace TeamCloud.Azure;

public interface IAzureSessionService
{
    Microsoft.Azure.Management.ResourceManager.Fluent.AzureEnvironment Environment { get; }

    IAzureSessionOptions Options { get; }

    Task<AZFluent.Azure.IAuthenticated> CreateSessionAsync();

    Task<AZFluent.IAzure> CreateSessionAsync(Guid subscriptionId);

    Task<string> AcquireTokenAsync(AzureEndpoint azureEndpoint = AzureEndpoint.ResourceManagerEndpoint);

    Task<IAzureSessionIdentity> GetIdentityAsync(AzureEndpoint azureEndpoint = AzureEndpoint.ResourceManagerEndpoint);

    Task<Guid> GetTenantIdAsync();

    Task<T> CreateClientAsync<T>(AzureEndpoint azureEndpoint = AzureEndpoint.ResourceManagerEndpoint, Guid? subscriptionId = null) where T : ServiceClient<T>;
}
