/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Flurl;
using Microsoft.Extensions.Caching.Memory;

namespace TeamCloud.Azure.Deployments.Providers
{
    public class MemoryArtifactsProvider : IAzureDeploymentArtifactsProvider
    {
        private readonly IMemoryArtifactsOptions memoryArtifactsOptions;
        private readonly IMemoryCache memoryCache;
        private readonly IAzureDeploymentTokenProvider azureDeploymentTokenProvider;

        public MemoryArtifactsProvider(IMemoryArtifactsOptions memoryArtifactsOptions, IMemoryCache memoryCache, IAzureDeploymentTokenProvider azureDeploymentTokenProvider)
        {
            this.memoryArtifactsOptions = memoryArtifactsOptions ?? throw new ArgumentNullException(nameof(memoryArtifactsOptions));
            this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            this.azureDeploymentTokenProvider = azureDeploymentTokenProvider ?? throw new ArgumentNullException(nameof(azureDeploymentTokenProvider));
        }

        public async Task<IAzureDeploymentArtifactsContainer> CreateContainerAsync(Guid deploymentId, IAzureDeploymentTemplate azureDeploymentTemplate)
        {
            var container = new Container();

            if (azureDeploymentTemplate.LinkedTemplates?.Any() ?? false)
            {
                foreach (var linkedTemplate in azureDeploymentTemplate.LinkedTemplates)
                {
                    var key = $"{nameof(MemoryArtifactsProvider)}|{deploymentId}|{linkedTemplate.Key}";
                    var ttl = DateTimeOffset.UtcNow.Add(TimeSpan.FromHours(1));

                    memoryCache.Set(key, linkedTemplate.Value, ttl);
                }

                container.Location = memoryArtifactsOptions.BaseUrl.AppendPathSegment(deploymentId).ToString();

                container.Token = await azureDeploymentTokenProvider
                    .AcquireToken(deploymentId, this)
                    .ConfigureAwait(false);
            }

            return container;
        }

        public sealed class Container : IAzureDeploymentArtifactsContainer
        {
            public string Location { get; internal set; }

            public string Token { get; internal set; }
        }
    }
}