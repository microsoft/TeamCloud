/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Flurl.Http;
using Microsoft.Azure.Management.ContainerInstance;
using Microsoft.Azure.Management.ContainerInstance.Models;
using Microsoft.Rest;

namespace TeamCloud.Azure.ContainerInstance;

public interface IContainerInstanceService
{
    Task<ContainerGroup> GetGroupAsync(string resourceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Usage>> GetUsageAsync(string subscriptionId, string location, CancellationToken cancellationToken = default);
    IAsyncEnumerable<Capabilities> GetCapabilitiesAsync(string subscriptionId, string location, CancellationToken cancellationToken = default);
    Task StopAsync(string resourceId, CancellationToken cancellationToken = default);
    Task<string> GetEventContentAsync(string resourceId, string containerName, CancellationToken cancellationToken = default);
    Task<string> GetLogsAsync(string groupResourceId, string containerName, CancellationToken cancellationToken = default);
}

public class ContainerInstanceService : IContainerInstanceService
{
    private readonly IArmService arm;

    public ContainerInstanceService(IArmService arm)
    {
        this.arm = arm ?? throw new ArgumentNullException(nameof(arm));
    }

    private async Task<ContainerInstanceManagementClient> GetClientAsync(string subscriptionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException($"'{nameof(subscriptionId)}' cannot be null or empty.", nameof(subscriptionId));

        var token = await arm
            .AcquireTokenAsync(cancellationToken)
            .ConfigureAwait(false);

        ServiceClientCredentials credentials = new TokenCredentials(token);

        return new ContainerInstanceManagementClient(credentials)
        {
            SubscriptionId = subscriptionId
        };
    }

    public async Task<ContainerGroup> GetGroupAsync(string resourceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(resourceId))
            throw new ArgumentException($"'{nameof(resourceId)}' cannot be null or empty.", nameof(resourceId));

        var id = new ResourceIdentifier(resourceId);

        var client = await GetClientAsync(id.SubscriptionId, cancellationToken)
            .ConfigureAwait(false);

        return await client.ContainerGroups
            .GetAsync(id.ResourceGroupName, id.Name, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Usage>> GetUsageAsync(string subscriptionId, string location, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException($"'{nameof(subscriptionId)}' cannot be null or empty.", nameof(subscriptionId));

        if (string.IsNullOrEmpty(location))
            throw new ArgumentException($"'{nameof(location)}' cannot be null or empty.", nameof(location));

        var client = await GetClientAsync(subscriptionId, cancellationToken)
            .ConfigureAwait(false);

        return await client.Location
            .ListUsageAsync(location, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<string> GetLogsAsync(string groupResourceId, string containerName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(groupResourceId))
            throw new ArgumentException($"'{nameof(groupResourceId)}' cannot be null or empty.", nameof(groupResourceId));

        if (string.IsNullOrEmpty(containerName))
            throw new ArgumentException($"'{nameof(containerName)}' cannot be null or empty.", nameof(containerName));

        var id = new ResourceIdentifier(groupResourceId);

        var client = await GetClientAsync(id.SubscriptionId, cancellationToken)
            .ConfigureAwait(false);

        var logs = await client.Containers
            .ListLogsAsync(id.ResourceGroupName, id.Name, containerName, null, cancellationToken)
            .ConfigureAwait(false);

        return logs.Content;
    }

    public async IAsyncEnumerable<Capabilities> GetCapabilitiesAsync(string subscriptionId, string location, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException($"'{nameof(subscriptionId)}' cannot be null or empty.", nameof(subscriptionId));

        if (string.IsNullOrEmpty(location))
            throw new ArgumentException($"'{nameof(location)}' cannot be null or empty.", nameof(location));

        // TODO: Not sure if the below is still valid, so leaving it here for now.

        // CAUTION - the ListCapabilitiesAync method of the ContainerInstanceManagementClient
        // has a serious issue and blows up the process when called - lets do the call using
        // Flurl but stick with the library's data object and we are happy

        // var token = await arm
        //     .AcquireTokenAsync(cancellationToken)
        //     .ConfigureAwait(false);

        // var capabilities = await arm.ArmEnvironment.Endpoint
        //     .AppendPathSegment($"subscriptions/{subscriptionId}/providers/Microsoft.ContainerInstance/locations/{location}/capabilities")
        //     .SetQueryParam("api-version", "2021-09-01")
        //     .WithOAuthBearerToken(token)
        //     .GetJsonAsync<IPage<Capabilities>>()
        //     .ConfigureAwait(false);

        var client = await GetClientAsync(subscriptionId, cancellationToken)
            .ConfigureAwait(false);

        var capabilities = await client.Location
            .ListCapabilitiesAsync(location, cancellationToken)
            .ConfigureAwait(false);

        while (true)
        {
            foreach (var capability in capabilities)
                yield return capability;

            if (string.IsNullOrEmpty(capabilities.NextPageLink))
                break;

            capabilities = await client.Location
                .ListCapabilitiesNextAsync(capabilities.NextPageLink, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public async Task StopAsync(string resourceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(resourceId))
            throw new ArgumentException($"'{nameof(resourceId)}' cannot be null or empty.", nameof(resourceId));

        var id = new ResourceIdentifier(resourceId);

        var client = await GetClientAsync(id.SubscriptionId, cancellationToken)
            .ConfigureAwait(false);

        await client.ContainerGroups
            .StopAsync(id.ResourceGroupName, id.Name, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<string> GetEventContentAsync(string resourceId, string containerName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(resourceId))
            throw new ArgumentException($"'{nameof(resourceId)}' cannot be null or empty.", nameof(resourceId));

        if (string.IsNullOrEmpty(containerName))
            throw new ArgumentException($"'{nameof(containerName)}' cannot be null or empty.", nameof(containerName));

        var id = new ResourceIdentifier(resourceId);

        var group = await GetGroupAsync(resourceId, cancellationToken)
            .ConfigureAwait(false);

        if (group is not null)
        {
            try
            {
                var lines = group.InstanceView.Events
                    .Where(e => e.LastTimestamp.HasValue)
                    .Select(e => new { timeStamp = e.LastTimestamp, message = FormatEventMessage(e) });

                var container = group.Containers.FirstOrDefault(c => c.Name == containerName);

                if (container?.InstanceView is not null)
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
