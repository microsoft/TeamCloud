/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Azure.Storage.Sas;
using Flurl.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using TeamCloud.Azure.Resources;
using TeamCloud.Azure.Resources.Typed;
using TeamCloud.Model.Data;

namespace TeamCloud.Data.Expanders
{
    public sealed class ComponentExpander : DocumentExpander,
        IDocumentExpander<Component>
    {
        private readonly IProjectRepository projectRepository;
        private readonly IAzureResourceService azureResourceService;
        private readonly IMemoryCache cache;

        public ComponentExpander(IProjectRepository projectRepository, IAzureResourceService azureResourceService, IMemoryCache cache) : base(true)
        {
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task ExpandAsync(Component document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            if (string.IsNullOrEmpty(document.ValueJson))
            {
                var project = await projectRepository
                    .GetAsync(document.Organization, document.ProjectId)
                    .ConfigureAwait(false);

                if (AzureResourceIdentifier.TryParse(project?.StorageId, out var storageId))
                {
                    var cacheKey = $"{GetType()}|{document.GetType()}|{document.Id}";

                    try
                    {
                        var sasUrl = await cache
                            .GetOrCreateAsync(cacheKey, AcquireSasUrl)
                            .ConfigureAwait(false);

                        if (sasUrl is null)
                        {
                            cache.Remove(cacheKey);
                        }
                        else
                        {
                            using var stream = await sasUrl.ToString()
                                .WithHeader("responsecontent-disposition", "file; attachment")
                                .WithHeader("responsecontent-type", "binary")
                                .GetStreamAsync()
                                .ConfigureAwait(false);

                            if ((stream?.Length ?? 0) > 0)
                            {
                                using var reader = new StreamReader(stream);

                                document.ValueJson = await reader
                                    .ReadToEndAsync()
                                    .ConfigureAwait(false);
                            }
                        }
                    }
                    catch
                    {
                        // swallow
                    }
                }

                async Task<Uri> AcquireSasUrl(ICacheEntry entry)
                {
                    var storageAccount = await azureResourceService
                        .GetResourceAsync<AzureStorageAccountResource>(storageId.ToString(), false)
                        .ConfigureAwait(false);

                    if (storageAccount is null)
                        return null;

                    entry.AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(1);

                    return await storageAccount
                        .CreateShareFileSasUriAsync(document.Id, "value.json", ShareFileSasPermissions.Read, entry.AbsoluteExpiration.Value.AddMinutes(5))
                        .ConfigureAwait(false);
                }
            }
        }
    }
}
