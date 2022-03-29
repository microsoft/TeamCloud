using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using TeamCloud.Azure.Resources.Utilities;

namespace TeamCloud.Azure.Resources.Typed
{
    public sealed class AzureAppServiceResource : AzureTypedResource
    {
        private readonly AsyncLazy<IWebApp> resource;

        public AzureAppServiceResource(string resourceId) : base("microsoft.web/sites", resourceId)
        {
            resource = new AsyncLazy<IWebApp>(() => GetResourceAsync());
        }

        private async Task<IWebApp> GetResourceAsync()
        {
            var session = await AzureResourceService.AzureSessionService
                .CreateSessionAsync(ResourceId.SubscriptionId)
                .ConfigureAwait(false);

            return await session.WebApps
                .GetByIdAsync(ResourceId.ToString())
                .ConfigureAwait(false);
        }

        public async Task UpdateContainerAsync()
        {
            var webApp = await resource.Value
                .ConfigureAwait(false);

            var profile = await webApp
                .GetPublishingProfileAsync()
                .ConfigureAwait(false);

            await $"https://{profile.GitUrl}"
                .AppendPathSegment("/docker/hook")
                .WithBasicAuth(profile.GitUsername, profile.GitPassword)
                .PostAsync()
                .ConfigureAwait(false);
        }
    }
}
