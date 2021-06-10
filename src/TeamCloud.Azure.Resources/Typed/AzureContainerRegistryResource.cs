/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.Management.ContainerRegistry.Fluent;
using TeamCloud.Azure.Resources.Utilities;
using TeamCloud.Http;

namespace TeamCloud.Azure.Resources.Typed
{
    public sealed class AzureContainerRegistryResource : AzureTypedResource
    {
        public static bool TryResolveFullyQualifiedContainerImageName(string containerImageName, out string resolvedContainerImageName)
        {
            try
            {
                resolvedContainerImageName = ResolveFullyQualifiedContainerImageName(containerImageName);

                return true;
            }
            catch
            {
                resolvedContainerImageName = default;

                return false;
            }
        }

        public static string ResolveFullyQualifiedContainerImageName(string containerImageName)
        {
            if (string.IsNullOrWhiteSpace(containerImageName))
                throw new ArgumentException($"'{nameof(containerImageName)}' cannot be null or whitespace.", nameof(containerImageName));

            containerImageName = containerImageName.Trim(); // do some input cleanup

            var index = containerImageName.IndexOf('/', StringComparison.OrdinalIgnoreCase);

            if (index == -1 || !StartsWithHostname(containerImageName))
            {
                containerImageName = $"docker.io/{containerImageName}";
            }

            index = containerImageName.LastIndexOf('/');

            foreach (var modeSeperator in new[] { '@', ':' })
            {
                containerImageName = containerImageName.TrimEnd(modeSeperator);

                if (containerImageName.LastIndexOf(modeSeperator) > index)
                {
                    return containerImageName; // image name is fully qualified
                }
            }

            return $"{containerImageName}:latest";

            bool StartsWithHostname(string containerImageName)
            {
                var containerImageHostname = containerImageName.Substring(0, containerImageName.IndexOf('/', StringComparison.OrdinalIgnoreCase));

                return containerImageHostname.Equals("localhost", StringComparison.OrdinalIgnoreCase) || containerImageName.IndexOfAny(new[] { '.', ':' }) >= 0;
            }
        }

        public static bool IsDockerHubContainerImage(string containerImageName)
        {
            var hostname = GetContainerHost(containerImageName);

            return hostname.Equals("docker.io", StringComparison.OrdinalIgnoreCase)
                || hostname.EndsWith($".docker.io", StringComparison.OrdinalIgnoreCase);
        }

        private static int GetContainerReferenceSeperatorIndex(string containerImageName)
        {
            var index = containerImageName.LastIndexOf('/');

            foreach (var seperator in new[] { '@', ':' })
            {
                var seperatorIndex = containerImageName.TrimEnd(seperator).LastIndexOf(seperator);

                if (seperatorIndex > index) return seperatorIndex;
            }

            return -1;
        }

        public static string GetContainerHost(string containerImageName)
        {
            containerImageName = ResolveFullyQualifiedContainerImageName(containerImageName);

            return containerImageName.Substring(0, containerImageName.IndexOf('/', StringComparison.OrdinalIgnoreCase));
        }

        public static string GetContainerName(string containerImageName)
        {
            containerImageName = ResolveFullyQualifiedContainerImageName(containerImageName);

            return containerImageName
                .Substring(0, GetContainerReferenceSeperatorIndex(containerImageName))
                .Substring(containerImageName.IndexOf('/', StringComparison.OrdinalIgnoreCase) + 1);
        }

        public static string GetContainerReference(string containerImageName)
        {
            containerImageName = ResolveFullyQualifiedContainerImageName(containerImageName);

            return containerImageName.Substring(GetContainerReferenceSeperatorIndex(containerImageName) + 1);
        }

        public static bool IsContainerImageNameDigestBased(string containerImageName)
        {
            containerImageName = ResolveFullyQualifiedContainerImageName(containerImageName);

            return (containerImageName.LastIndexOf('@') > containerImageName.LastIndexOf('/'));
        }

        public static bool IsContainerImageNameTagBased(string containerImageName)
        {
            containerImageName = ResolveFullyQualifiedContainerImageName(containerImageName);

            return (containerImageName.LastIndexOf(':') > containerImageName.LastIndexOf('/'));
        }

        public static string ChangeContainerHost(string containerImageName, string hostname)
        {
            containerImageName = ResolveFullyQualifiedContainerImageName(containerImageName);

            if (string.IsNullOrWhiteSpace(hostname))
                throw new ArgumentException($"'{nameof(hostname)}' cannot be null or whitespace.", nameof(hostname));

            return $"{hostname}{containerImageName.Substring(containerImageName.IndexOf('/', StringComparison.OrdinalIgnoreCase))}";
        }

        public static string ChangeContainerReference(string containerImageName, string reference)
        {
            containerImageName = ResolveFullyQualifiedContainerImageName(containerImageName);

            var containerImageNameBase = containerImageName.Substring(0, GetContainerReferenceSeperatorIndex(containerImageName));

            if (string.IsNullOrWhiteSpace(reference))
                throw new ArgumentException($"'{nameof(reference)}' cannot be null or whitespace.", nameof(reference));

            if (reference.StartsWith('@') || reference.StartsWith(':'))
                return string.Concat(containerImageNameBase, reference);

            return reference.Contains(':', StringComparison.OrdinalIgnoreCase)
                ? $"{containerImageNameBase}@{reference}"
                : $"{containerImageNameBase}:{reference}";
        }

        private readonly AsyncLazy<IRegistry> registryInstance;

        public AzureContainerRegistryResource(string resourceId) : base("Microsoft.ContainerRegistry/registries", resourceId)
        {
            registryInstance = new AsyncLazy<IRegistry>(() => GetRegistryAsync());
        }

        private async Task<IRegistry> GetRegistryAsync()
        {
            var session = await AzureResourceService.AzureSessionService
                .CreateSessionAsync(ResourceId.SubscriptionId)
                .ConfigureAwait(false);

            return await session.ContainerRegistries
                .GetByIdAsync(ResourceId.ToString())
                .ConfigureAwait(false);
        }

        public async Task ImportContainerImageAsync(string containerImageName, IEnumerable<string> containerImageTags = null, NetworkCredential credentials = null, bool force = false)
        {
            if (string.IsNullOrWhiteSpace(containerImageName))
                throw new ArgumentException($"'{nameof(containerImageName)}' cannot be null or whitespace.", nameof(containerImageName));

            if (!(containerImageTags?.Any() ?? false))
            {
                if (IsContainerImageNameDigestBased(containerImageName))
                    throw new ArgumentException($"'{nameof(containerImageTags)}' cannot be null or empty when '{nameof(containerImageName)}' is digest based.", nameof(containerImageTags));
                else
                    containerImageTags = Enumerable.Repeat(GetContainerReference(containerImageName), 1);
            }

            var containerImageNameBase = GetContainerName(containerImageName);

            containerImageTags = containerImageTags
                .Select(t => t.Contains(':', StringComparison.OrdinalIgnoreCase) ? t : $"{containerImageNameBase}:{t}")
                .ToArray();

            try
            {
                if (IsDockerHubContainerImage(containerImageName))
                    containerImageName = ChangeContainerHost(containerImageName, "docker.io");

                var payload = new
                {
                    source = new
                    {
                        credentials = credentials is null ? null : new
                        {
                            username = credentials.UserName,
                            password = credentials.Password
                        },
                        registryUri = GetContainerHost(containerImageName),
                        sourceImage = GetContainerName(containerImageName)
                    },
                    targetTags = containerImageTags,
                    untaggedTargetRepositories = Array.Empty<string>(),
                    mode = force ? "Force" : "NoForce"
                };

                var token = await AzureResourceService.AzureSessionService
                    .AcquireTokenAsync()
                    .ConfigureAwait(false);

                _ = await AzureResourceService.AzureSessionService
                    .Environment.ResourceManagerEndpoint
                    .AppendPathSegments(ResourceId.ToString(), "importImage")
                    .SetQueryParam("api-version", "2019-05-01")
                    .AllowAnyHttpStatus()
                    .WithOAuthBearerToken(token)
                    .PostJsonAsync(payload)
                    .ConfigureAwait(false);
            }
            catch (FlurlHttpException exc)
            {
                throw;
            }
            finally
            {
                registryInstance.Reset();
            }
        }

        public async Task<string> GetContainerImageDigestAsync(string containerImageName)
        {
            if (!IsContainerImageNameTagBased(containerImageName))
                throw new ArgumentException($"'{nameof(containerImageName)}' contain a tag based reference.", nameof(containerImageName));

            containerImageName = ResolveFullyQualifiedContainerImageName(containerImageName);

            var tokenScope = $"repository:{GetContainerName(containerImageName)}:pull";
            var token = await AcquireAccessTokenAsync(tokenScope).ConfigureAwait(false);

            var response = await $"https://{Hostname}/acr/v1/{GetContainerName(containerImageName)}/_tags/{GetContainerReference(containerImageName)}"
                .WithOAuthBearerToken(token)
                .AllowHttpStatus(HttpStatusCode.NotFound)
                .GetAsync()
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response
                    .ReadAsJsonAsync()
                    .ConfigureAwait(false);

                return responseJson.SelectToken("tag.digest")?.ToString();
            }

            return null;
        }

        public async IAsyncEnumerable<string> GetContainerImageTagsAsync(string containerImageName)
        {
            if (!GetContainerHost(containerImageName).Equals(Hostname, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"'{nameof(containerImageName)}' must start with '{Hostname}'.", nameof(containerImageName));

            if (AzureContainerRegistryResource.IsContainerImageNameTagBased(containerImageName))
            {
                var digest = await GetContainerImageDigestAsync(containerImageName).ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(digest))
                    throw new ArgumentException($"Failed to resolve digest for argument '{nameof(containerImageName)}'.", nameof(containerImageName));

                containerImageName = AzureContainerRegistryResource.ChangeContainerReference(containerImageName, digest);
            }

            var containerName = AzureContainerRegistryResource.GetContainerName(containerImageName);
            var containerDigest = AzureContainerRegistryResource.GetContainerReference(containerImageName);

            var tokenScope = $"repository:{GetContainerName(containerImageName)}:pull";
            var token = await AcquireAccessTokenAsync(tokenScope).ConfigureAwait(false);

            var json = await $"https://{Hostname}/acr/v1/{containerName}/_tags?digest={containerDigest}"
                .WithOAuthBearerToken(token)
                .GetJObjectAsync()
                .ConfigureAwait(false);

            foreach (var tag in json.SelectTokens("tags[].name"))
                yield return tag.ToString();
        }

        public async Task<bool> ContainesContainerImageAsync(string containerImageName)
        {
            var exists = await GetContainerImageTagsAsync(containerImageName)
                .AnyAsync()
                .ConfigureAwait(false);

            return exists;
        }

        public async Task<string> AcquireRefreshTokenAsync()
        {
            var registry = await registryInstance
                .ConfigureAwait(false);

            var tenantId = await AzureResourceService.AzureSessionService
                .GetTenantIdAsync()
                .ConfigureAwait(false);

            var token = await AzureResourceService.AzureSessionService
                .AcquireTokenAsync()
                .ConfigureAwait(false);

            var payload = new
            {
                grant_type = "access_token",
                service = registry.LoginServerUrl,
                tenant = tenantId.ToString(),
                access_token = token
            };

            var response = await $"https://{registry.LoginServerUrl}/oauth2/exchange"
                .PostUrlEncodedAsync(payload)
                .ConfigureAwait(false);

            var responseJson = await response.Content
                .ReadAsJsonAsync()
                .ConfigureAwait(false);

            return responseJson.SelectToken("refresh_token").ToString();
        }

        public async Task<string> AcquireAccessTokenAsync(string scope, string refreshToken = default)
        {
            if (string.IsNullOrWhiteSpace(scope))
                throw new ArgumentException($"'{nameof(scope)}' cannot be null or whitespace.", nameof(scope));

            if (string.IsNullOrWhiteSpace(refreshToken))
                refreshToken = await AcquireRefreshTokenAsync().ConfigureAwait(false);

            var registry = await registryInstance
               .ConfigureAwait(false);

            var payload = new
            {
                grant_type = "refresh_token",
                service = registry.LoginServerUrl,
                scope = scope,
                refresh_token = refreshToken
            };

            var response = await $"https://{registry.LoginServerUrl}/oauth2/token"
                .PostUrlEncodedAsync(payload)
                .ConfigureAwait(false);

            var responseJson = await response.Content
                .ReadAsJsonAsync()
                .ConfigureAwait(false);

            return responseJson.SelectToken("access_token").ToString();
        }

        public async Task<NetworkCredential> GetCredentialsAsync(string containerImageName = null)
        {
            var registry = await registryInstance
                .ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(containerImageName) && Hostname.Equals(GetContainerHost(containerImageName), StringComparison.OrdinalIgnoreCase))
            {
                var token = await AcquireAccessTokenAsync($"repository:{GetContainerName(containerImageName)}:pull")
                    .ConfigureAwait(false);

                return new NetworkCredential(Guid.Empty.ToString(),
                                             token,
                                             registry.LoginServerUrl);

            }
            else if (registry.AdminUserEnabled)
            {
                var credentials = await registry
                    .GetCredentialsAsync()
                    .ConfigureAwait(false);

                return new NetworkCredential(credentials.Username,
                                             credentials.AccessKeys.Select(ak => ak.Value).FirstOrDefault(),
                                             registry.LoginServerUrl);
            }

            return null;
        }

        public string Hostname { get => registryInstance.Value.Result.LoginServerUrl; }


    }
}
