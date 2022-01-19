/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Flurl;

namespace TeamCloud.Adapters.AzureDevOps;

public sealed class AzureDevOpsIdentifier
{
    private const string DEFAULT_HOSTNAME = "dev.azure.com";
    private const string PROJECTS_SEGMENT = "projects";

    private static readonly Regex HostportExpression = new Regex(@"(?<host>[^\/$]+?)(?<portsuffix>:(?<port>\d+))?$", RegexOptions.Compiled);
    private static readonly Regex ProjectExpression = new Regex(@"^\/organization\/(.+)\/project\/(.+)\/$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ResourceExpression = new Regex(@"^\/organization\/(.+)\/project\/(.+)\/(.+)\/$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static bool IsServiceHostname(string hostname)
        => hostname.Equals(DEFAULT_HOSTNAME, StringComparison.OrdinalIgnoreCase) || hostname.EndsWith(".visualstudio.com", StringComparison.OrdinalIgnoreCase);

    private static string SanitizeResourceId(string resourceId, out bool addedTrailingSlash)
    {
        addedTrailingSlash = false;
        resourceId = resourceId?.Trim();

        if (!string.IsNullOrEmpty(resourceId) && !resourceId.EndsWith("/", StringComparison.OrdinalIgnoreCase))
        {
            addedTrailingSlash = true;
            resourceId += "/";
        }

        return resourceId;
    }

    public static AzureDevOpsIdentifier FromUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException($"'{nameof(url)}' cannot be null or whitespace.", nameof(url));

        if (!Uri.TryCreate(url, UriKind.Absolute, out var urlParsed))
            throw new ArgumentException($"{nameof(url)} must be an absolute URL.", nameof(url));

        var segments = urlParsed.Segments
            .Select(s => s.Trim('/'))
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => HttpUtility.UrlDecode(s));

        if (!IsServiceHostname(urlParsed.Host))
        {
            segments = segments
                .SkipWhile(s => !s.Equals("tfs", StringComparison.OrdinalIgnoreCase));
        }

        var coreSegments = segments
            .TakeWhile(s => !s.Equals("_apis", StringComparison.OrdinalIgnoreCase));

        var areaSegments = segments
            .SkipWhile(s => !s.Equals("_apis", StringComparison.OrdinalIgnoreCase))
            .Skip(1); // skip one item to get rid of _apis in enumeration

        AzureDevOpsIdentifier identifier;

        if (IsServiceHostname(urlParsed.Host))
        {
            if (urlParsed.Host.Equals(DEFAULT_HOSTNAME, StringComparison.OrdinalIgnoreCase))
            {
                identifier = new AzureDevOpsIdentifier()
                {
                    Organization = coreSegments.ElementAtOrDefault(0),
                    Project = coreSegments.ElementAtOrDefault(1)
                };
            }
            else
            {
                identifier = new AzureDevOpsIdentifier()
                {
                    Organization = urlParsed.Host.Substring(0, urlParsed.Host.IndexOf(".", StringComparison.OrdinalIgnoreCase)),
                    Project = coreSegments.ElementAtOrDefault(1)
                };
            }
        }
        else
        {
            identifier = new AzureDevOpsIdentifier()
            {
                Organization = coreSegments.ElementAtOrDefault(0),
                Project = coreSegments.ElementAtOrDefault(1)
            };
        }

        if (PROJECTS_SEGMENT.Equals(areaSegments.FirstOrDefault(), StringComparison.OrdinalIgnoreCase))
        {
            identifier.Project ??= areaSegments.Skip(1).FirstOrDefault();
            identifier.ResourcePath = string.Join('/', areaSegments.Skip(2));
        }
        else
        {
            identifier.ResourcePath = string.Join('/', areaSegments.Skip(1));
        }

        return identifier;
    }

    public static AzureDevOpsIdentifier Parse(string resourceId)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
            throw new ArgumentException($"'{nameof(resourceId)}' cannot be null or whitespace.", nameof(resourceId));

        if (TryParse(resourceId, out var azureDevOpsIdentifier))
            return azureDevOpsIdentifier;

        throw new ArgumentException($"The given string is not a valid Azure DevOps resoure id.", nameof(resourceId));
    }

    public static bool TryParse(string resourceId, out AzureDevOpsIdentifier azureDevOpsIdentifier)
    {
        azureDevOpsIdentifier = null;

        if (string.IsNullOrWhiteSpace(resourceId))
            return false;

        foreach (var expression in new Regex[] { ResourceExpression, ProjectExpression })
        {
            var match = expression.Match(SanitizeResourceId(resourceId, out var _));

            if (match.Success)
            {
                azureDevOpsIdentifier = expression switch
                {
                    Regex exp when (expression == ProjectExpression) => new AzureDevOpsIdentifier()
                    {
                        Organization = match.Groups[1].Value,
                        Project = match.Groups[2].Value
                    },

                    Regex exp when (expression == ResourceExpression) => new AzureDevOpsIdentifier()
                    {
                        Organization = match.Groups[1].Value,
                        Project = match.Groups[2].Value,
                        ResourcePath = match.Groups[3].Value
                    },

                    _ => default
                };
            }

            if (azureDevOpsIdentifier is not null) break;
        }

        return (azureDevOpsIdentifier is not null);
    }

    public string Organization { get; set; }

    public string Project { get; set; }

    public string ResourcePath { get; set; }

    public string ResourceArea
        => ResourcePath?.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

    public IEnumerable<KeyValuePair<string, string>> ResourcePairs
    {
        get
        {
            var segments = ResourcePath?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

            for (int i = 1; i < segments.Length; i++) // skip the first item as it contains the api area
                yield return new KeyValuePair<string, string>(segments[i], segments.ElementAtOrDefault(++i));
        }
    }

    public bool TryGetResourceValue(string key, out string value)
        => TryGetResourceValue(key, false, out value);

    public bool TryGetResourceValue(string key, bool ignoreCase, out string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException($"'{nameof(key)}' cannot be null or whitespace.", nameof(key));

        value = ResourcePairs.FirstOrDefault(kvp => kvp.Key.Equals(key, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)).Value;

        return (value is not null);
    }

    public override string ToString()
    {
        var resourceId = "/"
            .AppendPathSegments("organization", Organization)
            .AppendPathSegments("project", Project);

        if (!string.IsNullOrEmpty(ResourcePath))
        {
            resourceId = resourceId
                .AppendPathSegments(ResourcePath);
        }

        return resourceId.ToString();
    }

    public string ToApiUrl()
        => ToApiUrl(DEFAULT_HOSTNAME);

    public string ToApiUrl(string hostname)
    {
        if (string.IsNullOrWhiteSpace(hostname))
            throw new ArgumentException($"'{nameof(hostname)}' cannot be null or whitespace.", nameof(hostname));

        if (!HostportExpression.TryMatch(hostname, "host", out var group) || Uri.CheckHostName(group.Value) == UriHostNameType.Unknown)
            throw new ArgumentException($"'{nameof(hostname)}' must be a valid hostname.", nameof(hostname));

        var url = IsServiceHostname(hostname)
            ? $"https://{hostname}".AppendPathSegment(Organization)
            : $"https://{hostname}".AppendPathSegments("tfs", Organization);

        if (!string.IsNullOrEmpty(ResourceArea))
        {
            url = url
                .AppendPathSegments("_apis", ResourcePath);
        }

        return url.ToString();
    }
}
