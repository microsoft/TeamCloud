/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Flurl;
using Microsoft.Azure.Management.ResourceManager.Fluent;

namespace TeamCloud.Azure.Resources
{
    public sealed class AzureResourceIdentifier
    {
        private static readonly Regex SubscriptionExpression = new Regex(@"^\/subscriptions\/(.*?)\/.*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ResourceGroupExpression = new Regex(@"^\/subscriptions\/(.*)\/resourcegroups\/(.*?)\/.*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ResourceExpression = new Regex(@"^\/subscriptions\/(.*)\/resourceGroups\/(.*)\/providers\/(.*?)\/(.*)\/$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ManagementExpression = new Regex(@"^\/providers\/(.*?)\/(.*)\/$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

        public static AzureResourceIdentifier Parse(string resourceId, bool allowUnnamedResource = false)
        {
            if (string.IsNullOrEmpty(resourceId))
                throw new ArgumentException("The resource id to parse must not NULL or EMPTY.", nameof(resourceId));

            if (TryParse(resourceId, allowUnnamedResource, out var azureResourceIdentifier))
                return azureResourceIdentifier;

            throw new ArgumentException($"The given string is not a valid Azure resoure id.", nameof(resourceId));
        }

        public static bool TryParse(string resourceId, out AzureResourceIdentifier azureResourceIdentifier)
            => TryParse(resourceId, false, out azureResourceIdentifier);

        public static bool TryParse(string resourceId, bool allowUnnamedResource, out AzureResourceIdentifier azureResourceIdentifier)
        {
            azureResourceIdentifier = null;

            if (string.IsNullOrEmpty(resourceId))
                return false;

            resourceId = SanitizeResourceId(resourceId, out bool addedTrailingSlash);

            foreach (var expression in new Regex[] { ResourceExpression, ResourceGroupExpression, SubscriptionExpression, ManagementExpression })
            {
                var match = expression.Match(resourceId);

                if (match.Success)
                {
                    if (Guid.TryParse(match.Groups[1].Value, out Guid subscriptionId))
                    {
                        if (expression == ResourceExpression)
                            azureResourceIdentifier = new AzureResourceIdentifier(subscriptionId, resourceGroup: match.Groups[2].Value, resourceNamespace: match.Groups[3].Value, resourceTypes: ParseResourceSegment(match.Groups[4].Value));
                        else if (expression == ResourceGroupExpression)
                            azureResourceIdentifier = new AzureResourceIdentifier(subscriptionId, resourceGroup: match.Groups[2].Value);
                        else if (expression == SubscriptionExpression)
                            azureResourceIdentifier = new AzureResourceIdentifier(subscriptionId);
                    }
                    else
                    {
                        if (expression == ManagementExpression)
                            azureResourceIdentifier = new AzureResourceIdentifier(Guid.Empty, resourceNamespace: match.Groups[1].Value, resourceTypes: ParseResourceSegment(match.Groups[2].Value));
                    }

                    if (azureResourceIdentifier != null) break;
                }
            }

            return (azureResourceIdentifier != null);

            KeyValuePair<string, string>[] ParseResourceSegment(string resourceSegment)
            {
                var tokens = resourceSegment.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                var segments = tokens
                    .Select((segment, index) => new { segment, index })
                    .GroupBy(x => x.index / 2, x => x.segment)
                    .Select(group => new KeyValuePair<string, string>(group.First(), group.Skip(1).LastOrDefault()));

                if (!allowUnnamedResource)
                {
                    var emptySegment = segments.FirstOrDefault(segment => string.IsNullOrEmpty(segment.Value));

                    if (!string.IsNullOrEmpty(emptySegment.Key))
                        throw new ArgumentException($"Missing name for resource '{emptySegment.Key}'", nameof(resourceId));
                }

                return segments.ToArray();
            }
        }

        public AzureResourceIdentifier(Guid subscriptionId, string resourceGroup = null, string resourceNamespace = null, params KeyValuePair<string, string>[] resourceTypes)
        {
            SubscriptionId = subscriptionId;
            ResourceGroup = resourceGroup;
            ResourceNamespace = resourceNamespace;
            ResourceTypes = new List<KeyValuePair<string, string>>(resourceTypes);
        }

        public Guid SubscriptionId { get; }
        public string ResourceGroup { get; }
        public string ResourceNamespace { get; }
        public IReadOnlyList<KeyValuePair<string, string>> ResourceTypes { get; }

        public string ResourceName
            => ResourceTypes.LastOrDefault().Value;

        public string ResourceTypeName
            => !string.IsNullOrEmpty(ResourceNamespace) && ResourceTypes.Any()
            ? string.Join('/', ResourceTypes.Select(kvp => kvp.Key))
            : null;

        public string ResourceTypeFullName
            => !string.IsNullOrEmpty(ResourceNamespace) && !string.IsNullOrEmpty(ResourceTypeName)
            ? $"{ResourceNamespace}/{ResourceTypeName}"
            : null;

        public override string ToString()
            => ToString(null);

        public string ToString(AzureResourceSegment? segment, int resourceCount = 0)
        {
            resourceCount = Math.Min(Math.Max(resourceCount, 0), ResourceTypes.Count);

            var resourceSegment = segment.GetValueOrDefault(AzureResourceSegment.Resource);
            var resourceId = new StringBuilder();

            if (Guid.Empty.Equals(SubscriptionId))
            {
                resourceId.Append($"/providers/{ResourceNamespace}");

                if (resourceSegment != AzureResourceSegment.Resource)
                {
                    throw new NotSupportedException($"Segment of type '{resourceSegment} is not supported for provider resource identifier.");
                }
                else if (ResourceTypes.Any())
                {
                    for (int i = 0; i < (resourceCount == 0 ? ResourceTypes.Count : resourceCount); i++)
                    {
                        resourceId.Append($"/{ResourceTypes[i].Key}/{ResourceTypes[i].Value}");
                    }
                }
            }
            else
            {
                resourceId.Append($"/subscriptions/{SubscriptionId}");

                if (resourceSegment != AzureResourceSegment.Subscription && !string.IsNullOrEmpty(ResourceGroup))
                {
                    resourceId.Append($"/resourceGroups/{ResourceGroup}");

                    if (resourceSegment != AzureResourceSegment.ResourceGroup && ResourceTypes.Any())
                    {
                        for (int i = 0; i < (resourceCount == 0 ? ResourceTypes.Count : resourceCount); i++)
                        {
                            if (i == 0) resourceId.Append($"/providers/{ResourceNamespace}");

                            resourceId.Append($"/{ResourceTypes[i].Key}/{ResourceTypes[i].Value}");
                        }
                    }
                }
            }

            return resourceId.ToString();
        }

        public string GetApiUrl(AzureEnvironment environment)
            => (environment ?? throw new ArgumentNullException(nameof(environment)))
            .ResourceManagerEndpoint.AppendPathSegment(this.ToString()).ToString();

        public string GetPortalUrl(Guid tenantId)
            => $"https://portal.azure.com/#@{tenantId}/resource{this}";
    }
}
