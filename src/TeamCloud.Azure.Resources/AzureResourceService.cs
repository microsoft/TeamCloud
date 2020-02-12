using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using TeamCloud.Http;

namespace TeamCloud.Azure.Resources
{

    public interface IAzureResourceService
    {
        IAzureSessionService AzureSessionService { get; }

        Task<AzureSubscription> GetSubscriptionAsync(Guid subscriptionId, bool throwIfNotExists = false);

        Task<AzureResourceGroup> GetResourceGroupAsync(Guid subscriptionId, string resourceGroupName, bool throwIfNotExists = false);

        Task<AzureResource> GetResourceAsync(string resourceId, bool throwIfNotExists = false);

        Task<IEnumerable<string>> GetApiVersionsAsync(Guid subscriptionId, string resourceNamespace, string resourceType, bool includePreviewVersions = false);
    }

    public class AzureResourceService : IAzureResourceService
    {
        public AzureResourceService(IAzureSessionService azureSessionService)
        {
            AzureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
        }

        public IAzureSessionService AzureSessionService { get; }

        public Task<AzureSubscription> GetSubscriptionAsync(Guid subscriptionId, bool throwIfNotExists = false)
            => AzureResource.InitializeAsync(new AzureSubscription(subscriptionId), this, throwIfNotExists);

        public Task<AzureResourceGroup> GetResourceGroupAsync(Guid subscriptionId, string resourceGroupName, bool throwIfNotExists = false)
            => AzureResource.InitializeAsync(new AzureResourceGroup(subscriptionId, resourceGroupName), this, throwIfNotExists);

        public Task<AzureResource> GetResourceAsync(string resourceId, bool throwIfNotExists = false)
            => AzureResource.InitializeAsync(new AzureResource(resourceId), this, throwIfNotExists);

        public async Task<IEnumerable<string>> GetApiVersionsAsync(Guid subscriptionId, string resourceNamespace, string resourceType, bool includePreviewVersions = false)
        {
            if (resourceNamespace is null)
                throw new ArgumentNullException(nameof(resourceNamespace));

            if (resourceType is null)
                throw new ArgumentNullException(nameof(resourceType));

            var token = await AzureSessionService
                .AcquireTokenAsync()
                .ConfigureAwait(false);

            var json = await AzureSessionService.Environment.ResourceManagerEndpoint
                .AppendPathSegment("subscriptions")
                .AppendPathSegment(subscriptionId)
                .AppendPathSegment("providers")
                .AppendPathSegment(resourceNamespace)
                .SetQueryParam("api-version", "2019-10-01")
                .WithOAuthBearerToken(token)
                .WithAzureResourceException(AzureSessionService.Environment)
                .GetJObjectAsync();

            var resourceTypeMatch = json
                .SelectTokens("$..resourceType")
                .Select(t => t.ToString())
                .FirstOrDefault(rt => rt.Equals(resourceType, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(resourceTypeMatch))
                throw new ArgumentOutOfRangeException(nameof(resourceType));

            var apiVersions = json
                .SelectTokens($"$..resourceTypes[?(@.resourceType == '{resourceType}')].apiVersions[*]")
                .Select(t => t.ToString());

            if (includePreviewVersions)
                return apiVersions;

            return apiVersions
                .Where(v => !v.EndsWith("-preview", StringComparison.OrdinalIgnoreCase));
        }


    }
}
