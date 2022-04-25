/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure.Containers.ContainerRegistry;
using Azure.Containers.ContainerRegistry.Specialized;
using Azure.Core;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.Management.ContainerRegistry;
using Microsoft.Azure.Management.ContainerRegistry.Models;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using TeamCloud.Http;
using static TeamCloud.Azure.ContainerRegistry.ContainerImageHelper;

namespace TeamCloud.Azure.ContainerRegistry;

public interface IContainerRegistryService
{
    // Task<IEnumerable<Usage>> GetUsagesAsync(string subscriptionId, string location, CancellationToken cancellationToken = default);
    // Task<IEnumerable<Capabilities>> GetCapabilitiesAsync(string subscriptionId, string location, CancellationToken cancellationToken = default);
    // Task StopAsync(string resourceId, CancellationToken cancellationToken = default);
    // Task<string> GetEventContentAsync(string resourceId, string containerName, CancellationToken cancellationToken = default);
}

public class ContainerRegistryService : IContainerRegistryService
{
    private readonly ConcurrentDictionary<string, ContainerRegistryClient> clientMap = new(StringComparer.OrdinalIgnoreCase);

    private readonly IArmService arm;

    public ContainerRegistryService(IArmService arm)
    {
        this.arm = arm ?? throw new ArgumentNullException(nameof(arm));
    }


    private async Task<ContainerRegistryManagementClient> GetManagementClientAsync(string subscriptionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException($"'{nameof(subscriptionId)}' cannot be null or empty.", nameof(subscriptionId));

        var token = await arm
            .AcquireTokenAsync(cancellationToken)
            .ConfigureAwait(false);

        ServiceClientCredentials credentials = new TokenCredentials(token);

        return new ContainerRegistryManagementClient(credentials)
        {
            SubscriptionId = subscriptionId
        };
    }

    public async Task<Registry> GetRegistryAsync(string resourceId, CancellationToken cancellationToken = default)
    {
        var id = new ResourceIdentifier(resourceId);

        var mgmtClient = await GetManagementClientAsync(id.SubscriptionId, cancellationToken)
            .ConfigureAwait(false);

        // mgmtClient.Registries.

        return await mgmtClient.Registries
            .GetAsync(id.ResourceGroupName, id.Name, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<ContainerRegistryClient> GetClientAsync(string resourceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(resourceId))
            throw new ArgumentException($"'{nameof(resourceId)}' cannot be null or empty.", nameof(resourceId));

        var id = new ResourceIdentifier(resourceId);

        if (!clientMap.TryGetValue(id, out var registryClient))
        {
            var registry = await GetRegistryAsync(resourceId, cancellationToken)
                .ConfigureAwait(false);

            var hostname = registry.LoginServer ?? $"{registry.Name ?? new ResourceIdentifier(resourceId).Name}.azurecr.io";
            var endpoint = new Uri($"https://{hostname}");

            registryClient = new ContainerRegistryClient(endpoint, arm.GetTokenCredential(), new ContainerRegistryClientOptions
            {
                Audience = ContainerRegistryAudience.AzureResourceManagerPublicCloud
            });

            clientMap[id] = registryClient;
        }

        return registryClient;
    }

    private async Task<Uri> GetEndpointAsync(string resourceId, CancellationToken cancellationToken = default)
    {
        var client = await GetClientAsync(resourceId, cancellationToken)
            .ConfigureAwait(false);

        return client.Endpoint;
    }

    public async System.Threading.Tasks.Task ImportContainerImageAsync(string resourceId, string containerImageName, IList<string> containerImageTags = null, NetworkCredential credentials = null, bool force = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(containerImageName))
            throw new ArgumentException($"'{nameof(containerImageName)}' cannot be null or whitespace.", nameof(containerImageName));

        if (!(containerImageTags?.Any() ?? false))
        {
            if (IsContainerImageNameDigestBased(containerImageName))
                throw new ArgumentException($"'{nameof(containerImageTags)}' cannot be null or empty when '{nameof(containerImageName)}' is digest based.", nameof(containerImageTags));
            else
                containerImageTags = Enumerable.Repeat(GetContainerReference(containerImageName), 1).ToList();
        }

        var containerImageNameBase = GetContainerName(containerImageName);

        containerImageTags = containerImageTags
            .Select(t => t.Contains(':', StringComparison.OrdinalIgnoreCase) ? t : $"{containerImageNameBase}:{t}")
            .ToArray();

        try
        {
            if (IsDockerHubContainerImage(containerImageName))
                containerImageName = ChangeContainerHost(containerImageName, "docker.io");

            var importImageParameters = new ImportImageParameters
            {
                Source = new ImportSource
                {
                    Credentials = credentials is null ? null : new ImportSourceCredentials
                    {
                        Username = credentials.UserName,
                        Password = credentials.Password
                    },
                    RegistryUri = GetContainerHost(containerImageName),
                    SourceImage = GetContainerName(containerImageName)
                },
                TargetTags = containerImageTags,
                UntaggedTargetRepositories = Array.Empty<string>(),
                Mode = force ? "Force" : "NoForce"
            };

            var id = new ResourceIdentifier(resourceId);

            var mgmtClient = await GetManagementClientAsync(id.SubscriptionId, cancellationToken)
                .ConfigureAwait(false);

            await mgmtClient.Registries
                .ImportImageAsync(id.ResourceGroupName, id.Name, importImageParameters, cancellationToken)
                .ConfigureAwait(false);


            // var payload = new
            // {
            //     source = new
            //     {
            //         credentials = credentials is null ? null : new
            //         {
            //             username = credentials.UserName,
            //             password = credentials.Password
            //         },
            //         registryUri = GetContainerHost(containerImageName),
            //         sourceImage = GetContainerName(containerImageName)
            //     },
            //     targetTags = containerImageTags,
            //     untaggedTargetRepositories = Array.Empty<string>(),
            //     mode = force ? "Force" : "NoForce"
            // };

            // var token = await azure
            //     .AcquireTokenAsync()
            //     .ConfigureAwait(false);

            // _ = await azure.ArmEnvironment.BaseUri
            //     .AppendPathSegments(resourceId, "importImage")
            //     .SetQueryParam("api-version", "2019-05-01")
            //     .AllowAnyHttpStatus()
            //     .WithOAuthBearerToken(token)
            //     .PostJsonAsync(payload)
            //     .ConfigureAwait(false);
        }
        catch (FlurlHttpException)
        {
            throw;
        }
        // finally
        // {
        //     registryInstance.Reset();
        // }
    }

    public async Task<string> GetContainerImageDigestAsync(string resourceId, string containerImageName)
    {
        if (!IsContainerImageNameTagBased(containerImageName))
            throw new ArgumentException($"'{nameof(containerImageName)}' contain a tag based reference.", nameof(containerImageName));

        containerImageName = ResolveFullyQualifiedContainerImageName(containerImageName);

        var tokenScope = $"repository:{GetContainerName(containerImageName)}:pull";
        var token = await AcquireAccessTokenAsync(resourceId, tokenScope).ConfigureAwait(false);



        var response = await $"https://{Hostname}/acr/v1/{GetContainerName(containerImageName)}/_tags/{GetContainerReference(containerImageName)}"
            .WithOAuthBearerToken(token)
            .AllowHttpStatus(HttpStatusCode.NotFound)
            .GetAsync()
            .ConfigureAwait(false);

        if (response.IsSuccessStatusCode())
        {
            var json = await response
                .GetJsonAsync<JObject>()
                .ConfigureAwait(false);

            return json.SelectToken("tag.digest")?.ToString();
        }

        return null;
    }

    public async IAsyncEnumerable<string> GetContainerImageTagsAsync(string resourceId, string containerImageName)
    {
        if (!GetContainerHost(containerImageName).Equals(Hostname, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"'{nameof(containerImageName)}' must start with '{Hostname}'.", nameof(containerImageName));

        if (IsContainerImageNameTagBased(containerImageName))
        {
            var digest = await GetContainerImageDigestAsync(resourceId, containerImageName).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(digest))
                throw new ArgumentException($"Failed to resolve digest for argument '{nameof(containerImageName)}'.", nameof(containerImageName));

            containerImageName = ChangeContainerReference(containerImageName, digest);
        }

        var containerName = GetContainerName(containerImageName);
        var containerDigest = GetContainerReference(containerImageName);

        var tokenScope = $"repository:{GetContainerName(containerImageName)}:pull";
        var token = await AcquireAccessTokenAsync(resourceId, tokenScope).ConfigureAwait(false);

        var json = await $"https://{Hostname}/acr/v1/{containerName}/_tags?digest={containerDigest}"
            .WithOAuthBearerToken(token)
            .GetJObjectAsync()
            .ConfigureAwait(false);

        foreach (var tag in json.SelectTokens("tags[].name"))
            yield return tag.ToString();
    }

    public async Task<bool> ContainesContainerImageAsync(string resourceId, string containerImageName)
    {
        var exists = await GetContainerImageTagsAsync(resourceId, containerImageName)
            .AnyAsync()
            .ConfigureAwait(false);

        return exists;
    }

    public async Task<string> AcquireRefreshTokenAsync(string resourceId, CancellationToken cancellationToken = default)
    {
        // var registry = await registryInstance
        //     .ConfigureAwait(false);

        var client = await GetClientAsync(resourceId, cancellationToken)
            .ConfigureAwait(false);

        var tenantId = await arm
            .GetTenantIdAsync(cancellationToken)
            .ConfigureAwait(false);

        var token = await arm
            .AcquireTokenAsync(cancellationToken)
            .ConfigureAwait(false);

        var payload = new
        {
            grant_type = "access_token",
            service = "TODO: UNCOMMENT",//registry.LoginServerUrl,
            tenant = tenantId,
            access_token = token
        };

        var response = await "TODO: UNCOMMENT"// $"https://{registry.LoginServerUrl}/oauth2/exchange"
            .PostUrlEncodedAsync(payload)
            .ConfigureAwait(false);

        var responseJson = await response
            .GetJsonAsync<JObject>()
            .ConfigureAwait(false);

        return responseJson.SelectToken("refresh_token").ToString();
    }

    public async Task<string> AcquireAccessTokenAsync(string resourceId, string scope, string refreshToken = default, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
            throw new ArgumentException($"'{nameof(resourceId)}' cannot be null or whitespace.", nameof(resourceId));

        if (string.IsNullOrWhiteSpace(scope))
            throw new ArgumentException($"'{nameof(scope)}' cannot be null or whitespace.", nameof(scope));

        if (string.IsNullOrWhiteSpace(refreshToken))
            refreshToken = await AcquireRefreshTokenAsync(resourceId, cancellationToken)
                .ConfigureAwait(false);


        // var registry = await registryInstance
        //    .ConfigureAwait(false);

        var payload = new
        {
            grant_type = "refresh_token",
            service = "TODO: UNCOMMENT",// registry.LoginServerUrl,
            scope,
            refresh_token = refreshToken
        };

        var response = await "TODO: UNCOMMENT"// $"https://{registry.LoginServerUrl}/oauth2/token"
            .PostUrlEncodedAsync(payload)
            .ConfigureAwait(false);

        var responseJson = await response
            .GetJsonAsync<JObject>()
            .ConfigureAwait(false);

        return responseJson.SelectToken("access_token").ToString();
    }

    public async Task<NetworkCredential> GetCredentialsAsync(string resourceId, string containerImageName = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
            throw new ArgumentException($"'{nameof(resourceId)}' cannot be null or whitespace.", nameof(resourceId));

        var client = await GetClientAsync(resourceId, cancellationToken)
            .ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(containerImageName) && Hostname.Equals(GetContainerHost(containerImageName), StringComparison.OrdinalIgnoreCase))
        {
            var token = await AcquireAccessTokenAsync(resourceId, $"repository:{GetContainerName(containerImageName)}:pull", cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            // return new NetworkCredential(Guid.Empty.ToString(), token, registry.LoginServerUrl);
            return new NetworkCredential(Guid.Empty.ToString(), token, client.Endpoint.AbsoluteUri);
        }
        else
        {
            var id = new ResourceIdentifier(resourceId);

            var mgmtClient = await GetManagementClientAsync(id.SubscriptionId, cancellationToken)
                .ConfigureAwait(false);

            var registry = await mgmtClient.Registries
                .GetAsync(id.ResourceGroupName, id.Name, cancellationToken)
                .ConfigureAwait(false);

            if (registry.AdminUserEnabled.HasValue && registry.AdminUserEnabled.Value)
            {
                var credentials = await mgmtClient.Registries
                    .ListCredentialsAsync(id.ResourceGroupName, id.Name, cancellationToken)
                    .ConfigureAwait(false);

                return new NetworkCredential(credentials.Username,
                                             credentials.Passwords.Select(p => p.Value).FirstOrDefault(),
                                             client.Endpoint.AbsoluteUri);

            }
        }
        // else if (registry.AdminUserEnabled)
        // {
        //     var credentials = await registry
        //         .GetCredentialsAsync()
        //         .ConfigureAwait(false);

        //     return new NetworkCredential(credentials.Username,
        //                                  credentials.AccessKeys.Select(ak => ak.Value).FirstOrDefault(),
        //                                  registry.LoginServerUrl);
        // }

        return null;
    }

    public string Hostname { get => "TODO: UNCOMMENT"; }// registryInstance.Value.Result.LoginServerUrl; }
}
