using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.ContainerInstance.Fluent.Models;
using Microsoft.Azure.Management.ContainerRegistry.Fluent;
using TeamCloud.Azure.Resources.Utilities;

namespace TeamCloud.Azure.Resources.Typed
{
    public sealed class AzureContainerGroupResource : AzureTypedResource
    {
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

            if (group != null)
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

            if (group != null)
            {
                try
                {
                    var lines = group.Events
                        .Where(e => e.LastTimestamp.HasValue)
                        .Select(e => new { timeStamp = e.LastTimestamp, message = FormatEventMessage(e) });

                    if (group.Containers.TryGetValue(containerName, out var container) && container.InstanceView != null)
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
