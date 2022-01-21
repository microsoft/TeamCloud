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
using Microsoft.Rest.Azure;
using TeamCloud.Http;

namespace TeamCloud.Azure.Resources;

public sealed class AzureSubscription : AzureResource
{
    private static string GetResourceId(Guid subscriptionId)
        => new AzureResourceIdentifier(subscriptionId).ToString();

    internal AzureSubscription(Guid subscriptionId)
        : base(GetResourceId(subscriptionId))
    { }

    public AzureSubscription(Guid subscriptionId, IAzureResourceService azureResourceService)
        : base(GetResourceId(subscriptionId), azureResourceService)
    { }

    public override async Task<bool> ExistsAsync()
    {
        var session = await AzureResourceService.AzureSessionService
            .CreateSessionAsync()
            .ConfigureAwait(false);

        try
        {
            _ = await session.Subscriptions
                .GetByIdAsync(this.ResourceId.SubscriptionId.ToString())
                .ConfigureAwait(false);

            return true;
        }
        catch (CloudException exc) when (exc.Response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task<AzureRoleAssignmentUsage> GetRoleAssignmentUsageAsync()
    {
        var token = await AzureResourceService.AzureSessionService
            .AcquireTokenAsync()
            .ConfigureAwait(false);

        return await AzureResourceService.AzureSessionService.Environment.ResourceManagerEndpoint
            .AppendPathSegment($"subscriptions/{this.ResourceId.SubscriptionId}/providers/Microsoft.Authorization/roleAssignmentsUsageMetrics")
            .SetQueryParam("api-version", "2019-08-01-preview")
            .WithOAuthBearerToken(token)
            .GetJsonAsync<AzureRoleAssignmentUsage>()
            .ConfigureAwait(false);
    }

    public async Task<AzureResourceGroup> CreateResourceGroupAsync(string name, string region = default)
    {
        var session = await AzureResourceService.AzureSessionService
            .CreateSessionAsync(this.ResourceId.SubscriptionId)
            .ConfigureAwait(false);

        _ = await session.ResourceGroups
            .Define(name)
            .WithRegion(region ?? AzureResourceService.AzureSessionService.Environment.Name)
            .CreateAsync()
            .ConfigureAwait(false);

        return new AzureResourceGroup(this.ResourceId.SubscriptionId, name, this.AzureResourceService);
    }

    public async IAsyncEnumerable<AzureResourceGroup> GetResourceGroupsAsync()
    {
        var session = await AzureResourceService.AzureSessionService
            .CreateSessionAsync(this.ResourceId.SubscriptionId)
            .ConfigureAwait(false);

        var resourceGroups = await session.ResourceGroups
            .ListAsync()
            .ConfigureAwait(false);

        await foreach (var resourceGroup in resourceGroups.AsContinuousCollectionAsync())
            yield return new AzureResourceGroup(this.ResourceId.SubscriptionId, resourceGroup.Name);
    }

    public override async Task<IDictionary<string, string>> GetTagsAsync(bool includeHidden = false)
    {
        var token = await AzureResourceService.AzureSessionService
            .AcquireTokenAsync()
            .ConfigureAwait(false);

        var json = await AzureResourceService.AzureSessionService.Environment.ResourceManagerEndpoint
            .AppendPathSegment($"subscriptions/{this.ResourceId.SubscriptionId}/providers/Microsoft.Resources/tags/default")
            .SetQueryParam("api-version", "2019-10-01")
            .WithOAuthBearerToken(token)
            .GetJObjectAsync()
            .ConfigureAwait(false);

        var tags = json
            .SelectToken("$.properties.tags")?
            .ToObject<Dictionary<string, string>>()
            ?? new Dictionary<string, string>();

        return tags
            .Where(kvp => includeHidden || !kvp.Key.StartsWith("hidden-", StringComparison.OrdinalIgnoreCase))
            .ToDictionary();
    }

    public override async Task SetTagsAsync(IDictionary<string, string> tags, bool merge = false)
    {
        if (merge && !(tags?.Any() ?? false))
            return; // nothing to merge

        if (merge)
        {
            var existingTags = await GetTagsAsync(true)
                .ConfigureAwait(false);

            tags = existingTags.Override(tags);
        }

        var token = await AzureResourceService.AzureSessionService
            .AcquireTokenAsync()
            .ConfigureAwait(false);

        var payload = new
        {
            operation = "Replace",
            properties = new
            {
                tags = tags.Where(tag => tag.Value is not null).ToDictionary()
            }
        };

        await AzureResourceService.AzureSessionService.Environment.ResourceManagerEndpoint
            .AppendPathSegment($"subscriptions/{this.ResourceId.SubscriptionId}/providers/Microsoft.Resources/tags/default")
            .SetQueryParam("api-version", "2019-10-01")
            .WithOAuthBearerToken(token)
            .PatchJsonAsync(payload)
            .ConfigureAwait(false);
    }

    public async IAsyncEnumerable<Region> GetRegionsAsync()
    {
        var token = await AzureResourceService.AzureSessionService
            .AcquireTokenAsync()
            .ConfigureAwait(false);

        var json = await AzureResourceService.AzureSessionService.Environment.ResourceManagerEndpoint
            .AppendPathSegment($"subscriptions/{this.ResourceId.SubscriptionId}/locations")
            .SetQueryParam("api-version", "2020-01-01")
            .WithOAuthBearerToken(token)
            .GetJObjectAsync()
            .ConfigureAwait(false);

        while (true)
        {
            foreach (var item in json.SelectTokens("value[*]"))
            {

                var paired = item
                    .SelectTokens("metadata.pairedRegion[*].name")
                    .Select(token => token.ToString())
                    .ToArray();

                yield return new Region()
                {
                    Name = item.SelectToken("name")?.ToString(),
                    DisplayName = item.SelectToken("displayName")?.ToString(),
                    Group = item.SelectToken("metadata.geographyGroup")?.ToString(),
                    Paired = paired
                };
            }

            var nextLink = json.SelectToken("nextLink")?.ToString();

            if (string.IsNullOrEmpty(nextLink))
                break;

            json = await nextLink
                .SetQueryParam("api-version", "2020-01-01")
                .WithOAuthBearerToken(token)
                .GetJObjectAsync()
                .ConfigureAwait(false);
        }
    }

    public sealed class Region
    {
        public string Name { get; internal set; }

        public string DisplayName { get; internal set; }

        public string Group { get; internal set; }

        public string[] Paired { get; internal set; }
    }
}
