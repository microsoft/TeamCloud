/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Core.Extensions;
using Azure.Core.Pipeline;
using Azure.Core.Serialization;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.ManagementGroups;
using Azure.ResourceManager.ManagementGroups.Models;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Resources.Models;
using Azure.ResourceManager.Storage;
using Azure.Security.KeyVault;
using Azure.Security.KeyVault.Secrets;
using Flurl.Http.Configuration;
using Microsoft.Identity.Client;
using TeamCloud.Azure.ContainerInstance;
using TeamCloud.Azure.KeyVault;
using TeamCloud.Azure.Storage;
using IHttpClientFactory = Flurl.Http.Configuration.IHttpClientFactory;

namespace TeamCloud.Azure;

public interface IArmService
{
    // IStorageService Storage { get; }
    ArmEnvironment ArmEnvironment { get; }
    Task<string> GetTenantIdAsync(CancellationToken cancellationToken = default);
    DefaultAzureCredential GetTokenCredential();
    ArmClient GetArmClient(string subscriptionId = null);
    Task<IAzureIdentity> GetIdentityAsync(CancellationToken cancellationToken = default);
    Task<string> AcquireTokenAsync(CancellationToken cancellationToken = default);
}

public class ArmService : IArmService
{
    private readonly ConcurrentDictionary<string, ArmClient> armClientMap = new(StringComparer.OrdinalIgnoreCase);

    private static bool IsAzureEnvironment =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));

    // public static Task<string> AcquireTokenAsync(AzureEndpoint azureEndpoint = AzureEndpoint.ResourceManagerEndpoint, IAzureSessionOptions azureSessionOptions = null, IHttpClientFactory httpClientFactory = null)
    //     => new AzureSessionService(azureSessionOptions, httpClientFactory).AcquireTokenAsync(azureEndpoint);

    private readonly IAzureSessionOptions azureSessionOptions;
    private readonly IHttpClientFactory httpClientFactory;

    public ArmService(
        IAzureSessionOptions azureSessionOptions = null,
        IHttpClientFactory httpClientFactory = null)
    {
        this.azureSessionOptions = azureSessionOptions ?? AzureSessionOptions.Default;
        this.httpClientFactory = httpClientFactory ?? new DefaultHttpClientFactory();
    }

    public ArmEnvironment ArmEnvironment => ArmEnvironment.AzurePublicCloud;

    // The DefaultAzureCredential will attempt to authenticate via the following mechanisms in order.
    //   1. Environment - The DefaultAzureCredential will read account information specified via environment
    //      variables and use it to authenticate. (AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_CLIENT_SECRET)
    //   2. Managed Identity - If the application is deployed to an Azure host with Managed Identity enabled,
    //      the DefaultAzureCredential will authenticate with that account.
    //   3. Visual Studio - If the developer has authenticated via Visual Studio, the DefaultAzureCredential
    //      will authenticate with that account.
    //   4. Visual Studio Code - If the developer has authenticated via the Visual Studio Code Azure Account
    //      plugin, the DefaultAzureCredential will authenticate with that account.
    //   5. Azure CLI - If the developer has authenticated an account via the Azure CLI az login command,
    //      the DefaultAzureCredential will authenticate with that account.
    //   6. Azure PowerShell - If the developer has authenticated an account via the Azure PowerShell
    //      Connect-AzAccount command, the DefaultAzureCredential will authenticate with that account.
    //   7. Interactive - If enabled the DefaultAzureCredential will interactively authenticate the developer
    //      via the current system's default browser. (disabled by default)
    public DefaultAzureCredential GetTokenCredential() => new();

    public ArmClient GetArmClient(string subscriptionId = null)
    {
        var subscriptionKey = subscriptionId ?? string.Empty;

        if (!armClientMap.TryGetValue(subscriptionKey, out var armClient))
        {
            armClient = new ArmClient(GetTokenCredential(), subscriptionId);

            armClientMap[subscriptionKey] = armClient;
        }

        return armClient;
    }

    private static string _tenantId;

    public async Task<string> GetTenantIdAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(_tenantId))
            return _tenantId;

        var envVar = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");

        if (envVar is not null && Guid.TryParse(envVar, out var tenantId))
        {
            _tenantId = tenantId.ToString();
        }
        else
        {
            var identity = await GetIdentityAsync(cancellationToken)
                .ConfigureAwait(false);

            _tenantId = identity.TenantId;
        }

        if (string.IsNullOrEmpty(_tenantId))
            throw new NotSupportedException();

        return _tenantId;
    }

    public IAzureSessionOptions Options { get => azureSessionOptions; }

    public async Task<IAzureIdentity> GetIdentityAsync(CancellationToken cancellationToken = default)
    {
        var token = await AcquireTokenAsync(cancellationToken)
            .ConfigureAwait(false);

        var jwtToken = new JwtSecurityTokenHandler()
            .ReadJwtToken(token);

        var identity = new AzureIdentity();

        if (jwtToken.Payload.TryGetValue("tid", out var tidValue) && Guid.TryParse(tidValue.ToString(), out Guid tid))
            identity.TenantId = tid.ToString();

        if (jwtToken.Payload.TryGetValue("oid", out var oidValue) && Guid.TryParse(oidValue.ToString(), out Guid oid))
            identity.ObjectId = oid.ToString();

        if (jwtToken.Payload.TryGetValue("appid", out var appidValue) && Guid.TryParse(appidValue.ToString(), out Guid appid))
            identity.ClientId = appid.ToString(); // version 1.0
        else if (jwtToken.Payload.TryGetValue("azp", out appidValue) && Guid.TryParse(appidValue.ToString(), out appid))
            identity.ClientId = appid.ToString(); // version 2.0

        return identity;
    }

    public async Task<string> AcquireTokenAsync(CancellationToken cancellationToken = default)
    {
        if (IsAzureEnvironment)
        {
            // ensure we disable SSL verfication for this process when using the Azure CLI to aqcuire MSI token.
            // otherwise our code will fail in dev scenarios where a dev proxy like fiddler is running to sniff
            // http traffix between our services or between service and other reset apis (e.g. Azure)
            Environment.SetEnvironmentVariable("AZURE_CLI_DISABLE_CONNECTION_VERIFICATION", "1", EnvironmentVariableTarget.Process);
        }

        var accessToken = await GetTokenCredential()
            .GetTokenAsync(new TokenRequestContext(scopes: new string[] { ArmEnvironment.DefaultScope }), cancellationToken)
            .ConfigureAwait(false);

        return accessToken.Token;
    }
}
