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

namespace TeamCloud.Azure.Resources
{
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
            var session = AzureResourceService.AzureSessionService.CreateSession();

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

        public async Task<AzureResourceGroup> CreateResourceGroupAsync(string name, string region = default)
        {
            var session = AzureResourceService.AzureSessionService
                .CreateSession(this.ResourceId.SubscriptionId);

            _ = await session.ResourceGroups
                .Define(name)
                .WithRegion(region ?? AzureResourceService.AzureSessionService.Environment.Name)
                .CreateAsync()
                .ConfigureAwait(false);

            return new AzureResourceGroup(this.ResourceId.SubscriptionId, name, this.AzureResourceService);
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
                    tags = tags.Where(tag => tag.Value != null).ToDictionary()
                }
            };

            await AzureResourceService.AzureSessionService.Environment.ResourceManagerEndpoint
                .AppendPathSegment($"subscriptions/{this.ResourceId.SubscriptionId}/providers/Microsoft.Resources/tags/default")
                .SetQueryParam("api-version", "2019-10-01")
                .WithOAuthBearerToken(token)
                .PatchJsonAsync(payload)
                .ConfigureAwait(false);
        }
    }
}
