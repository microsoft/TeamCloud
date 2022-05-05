/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Azure.ContainerRegistry;

public static class ContainerImageHelper
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

        static bool StartsWithHostname(string containerImageName)
        {
            var containerImageHostname = containerImageName[..containerImageName.IndexOf('/', StringComparison.OrdinalIgnoreCase)];

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

        return containerImageName[..containerImageName.IndexOf('/', StringComparison.OrdinalIgnoreCase)];
    }

    public static string GetContainerName(string containerImageName)
    {
        containerImageName = ResolveFullyQualifiedContainerImageName(containerImageName);

        return containerImageName[..GetContainerReferenceSeperatorIndex(containerImageName)][(containerImageName.IndexOf('/', StringComparison.OrdinalIgnoreCase) + 1)..];
    }

    public static string GetContainerReference(string containerImageName)
    {
        containerImageName = ResolveFullyQualifiedContainerImageName(containerImageName);

        return containerImageName[(GetContainerReferenceSeperatorIndex(containerImageName) + 1)..];
    }

    public static bool IsContainerImageNameDigestBased(string containerImageName)
    {
        containerImageName = ResolveFullyQualifiedContainerImageName(containerImageName);

        return containerImageName.LastIndexOf('@') > containerImageName.LastIndexOf('/');
    }

    public static bool IsContainerImageNameTagBased(string containerImageName)
    {
        containerImageName = ResolveFullyQualifiedContainerImageName(containerImageName);

        return containerImageName.LastIndexOf(':') > containerImageName.LastIndexOf('/');
    }

    public static string ChangeContainerHost(string containerImageName, string hostname)
    {
        containerImageName = ResolveFullyQualifiedContainerImageName(containerImageName);

        if (string.IsNullOrWhiteSpace(hostname))
            throw new ArgumentException($"'{nameof(hostname)}' cannot be null or whitespace.", nameof(hostname));

        return $"{hostname}{containerImageName[containerImageName.IndexOf('/', StringComparison.OrdinalIgnoreCase)..]}";
    }

    public static string ChangeContainerReference(string containerImageName, string reference)
    {
        containerImageName = ResolveFullyQualifiedContainerImageName(containerImageName);

        var containerImageNameBase = containerImageName[..GetContainerReferenceSeperatorIndex(containerImageName)];

        if (string.IsNullOrWhiteSpace(reference))
            throw new ArgumentException($"'{nameof(reference)}' cannot be null or whitespace.", nameof(reference));

        if (reference.StartsWith('@') || reference.StartsWith(':'))
            return string.Concat(containerImageNameBase, reference);

        return reference.Contains(':', StringComparison.OrdinalIgnoreCase)
            ? $"{containerImageNameBase}@{reference}"
            : $"{containerImageNameBase}:{reference}";
    }
}