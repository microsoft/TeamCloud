using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.ContainerInstance.Fluent.Models;
using Microsoft.Azure.Management.ContainerRegistry.Fluent;
using Newtonsoft.Json;
using TeamCloud.Azure.Resources.Utilities;
using TeamCloud.Http;
using TeamCloud.Serialization;

namespace TeamCloud.Azure.Resources.Typed
{
    public sealed class AzureContainerGroupResource : AzureTypedResource
    {
        public static async Task<IEnumerable<Usage>> GetUsageAsync(IAzureResourceService azureResourceService, Guid subscriptionId, string region)
        {
            if (azureResourceService is null)
                throw new ArgumentNullException(nameof(azureResourceService));

            if (string.IsNullOrEmpty(region))
                throw new ArgumentException($"'{nameof(region)}' cannot be null or empty.", nameof(region));

            var client = await azureResourceService.AzureSessionService
                .CreateClientAsync<ContainerInstanceManagementClient>(subscriptionId: subscriptionId)
                .ConfigureAwait(false);

            var usage = await client.ContainerGroupUsage
                .ListAsync(region)
                .ConfigureAwait(false);

            return usage.Value;
        }

        public static async IAsyncEnumerable<Capabilities> GetCapabilitiesAsync(IAzureResourceService azureResourceService, Guid subscriptionId, string region)
        {
            if (azureResourceService is null)
                throw new ArgumentNullException(nameof(azureResourceService));

            if (string.IsNullOrEmpty(region))
                throw new ArgumentException($"'{nameof(region)}' cannot be null or empty.", nameof(region));

            // CAUTION - the ListCapabilitiesAync method of the ContainerInstanceManagementClient
            // has a serious issue and blows up the process when called - lets do the call using
            // Flurl but stick with the library's data object and we are happy

            var token = await azureResourceService.AzureSessionService
                .AcquireTokenAsync()
                .ConfigureAwait(false);

            var capabilities = await azureResourceService.AzureSessionService.Environment.ResourceManagerEndpoint
                .AppendPathSegment($"subscriptions/{subscriptionId}/providers/Microsoft.ContainerInstance/locations/{region}/capabilities")
                .SetQueryParam("api-version", "2019-12-01")
                .WithOAuthBearerToken(token)
                .GetJsonAsync<CapabilitiesListResultInner>()
                .ConfigureAwait(false);

            while (true)
            {
                foreach (var capability in capabilities.Value ?? Enumerable.Empty<Capabilities>())
                    yield return capability;

                if (string.IsNullOrEmpty(capabilities.NextLink))
                    break;

                capabilities = await capabilities.NextLink
                    .WithOAuthBearerToken(token)
                    .GetJsonAsync<CapabilitiesListResultInner>()
                    .ConfigureAwait(false);
            }
        }

        private readonly AsyncLazy<IContainerGroup> containerGroup;

        public AzureContainerGroupResource(string resourceId) : base("Microsoft.ContainerInstance/containerGroups", resourceId)
        {
            containerGroup = new AsyncLazy<IContainerGroup>(() => GetGroupAsync());
        }

        private async Task<IContainerGroup> GetGroupAsync()
        {
            var session = await AzureResourceService.AzureSessionService
                .CreateSessionAsync(ResourceId.SubscriptionId)
                .ConfigureAwait(false);

            return await session.ContainerGroups
                .GetByIdAsync(ResourceId.ToString())
                .ConfigureAwait(false);
        }

        public async Task<string> GetLogContentAsync(string containerName)
        {
            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentException($"'{nameof(containerName)}' cannot be null or whitespace.", nameof(containerName));

            var group = await containerGroup
                .ConfigureAwait(false);

            if (group is not null)
            {
                try
                {
                    return await group
                        .GetLogContentAsync(containerName)
                        .ConfigureAwait(false);
                }
                catch
                {
                    // swallow
                }
            }

            return default;
        }

        public async Task<string> GetEventContentAsync(string containerName)
        {
            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentException($"'{nameof(containerName)}' cannot be null or whitespace.", nameof(containerName));

            var group = await containerGroup
                .ConfigureAwait(false);

            if (group is not null)
            {
                try
                {
                    var lines = group.Events
                        .Where(e => e.LastTimestamp.HasValue)
                        .Select(e => new { timeStamp = e.LastTimestamp, message = FormatEventMessage(e) });

                    if (group.Containers.TryGetValue(containerName, out var container) && container.InstanceView is not null)
                    {
                        lines = lines.Concat(container.InstanceView.Events
                            .Where(e => e.LastTimestamp.HasValue)
                            .Select(e => new { timeStamp = e.LastTimestamp, message = FormatEventMessage(e) }));

                    }

                    return string.Join(Environment.NewLine, lines.OrderBy(line => line.timeStamp).Select(line => line.message));
                }
                catch
                {
                    // swallow
                }
            }

            return default;

            static string FormatEventMessage(EventModel evt)
                => $"{evt.LastTimestamp.Value:yyyy-MM-dd hh:mm:ss}\t{evt.Name}\t\t{evt.Message}";
        }
    }
}
