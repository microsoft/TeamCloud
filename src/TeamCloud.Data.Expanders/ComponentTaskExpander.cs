/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Extensions.Caching.Memory;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TeamCloud.Azure.Resources;
using TeamCloud.Azure.Resources.Typed;
using TeamCloud.Model.Data;
using Flurl;
using Flurl.Http;
using System.Net;
using Azure.Storage.Sas;

namespace TeamCloud.Data.Expanders
{
    public sealed class ComponentTaskExpander : DocumentExpander,
        IDocumentExpander<ComponentTask>
    {
        private readonly IProjectRepository projectRepository;
        private readonly IAzureResourceService azureResourceService;
        private readonly IMemoryCache cache;

        public ComponentTaskExpander(IProjectRepository projectRepository, IAzureResourceService azureResourceService, IMemoryCache cache) : base(true)
        {
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task ExpandAsync(ComponentTask document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            if (string.IsNullOrEmpty(document.Output))
            {
                var results = await Task.WhenAll(

                    GetEventsAsync(document),
                    GetOutputAsync(document)

                ).ConfigureAwait(false);

                if (results.Any(result => !string.IsNullOrEmpty(result)))
                {
                    document.Output = string.Join(Environment.NewLine, results); // do some empty line trimming (left & right)
                    document.Output = Regex.Replace(document.Output, @"^([\s])*", string.Empty, RegexOptions.Singleline);
                    document.Output = Regex.Replace(document.Output, @"([\s])*$", string.Empty, RegexOptions.Singleline);
                }
            }
        }

        private async Task<string> GetEventsAsync(ComponentTask document)
        {
            if (AzureResourceIdentifier.TryParse(document.ResourceId, out var resourceId))
            {
                try
                {
                    var session = await azureResourceService.AzureSessionService
                        .CreateSessionAsync(resourceId.SubscriptionId)
                        .ConfigureAwait(false);

                    var runner = await session.ContainerGroups
                        .GetByIdAsync(resourceId.ToString())
                        .ConfigureAwait(false);

                    var container = runner?.Containers
                        .SingleOrDefault()
                        .Value;

                    if (container?.InstanceView != null)
                    {
                        var lines = container.InstanceView.Events
                            .Where(e => e.LastTimestamp.HasValue)
                            .OrderBy(e => e.LastTimestamp)
                            .Select(e => $"{e.LastTimestamp.Value:yyyy-MM-dd hh:mm:ss}\t{e.Name}\t\t{e.Message}");

                        if (lines.Any())
                            lines = lines.Append(string.Empty);

                        return string.Join(Environment.NewLine, lines);
                    }
                }
                catch
                {
                    // swallow
                }
            }

            return default;
        }

        private async Task<string> GetOutputAsync(ComponentTask document)
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

                        return null;
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

                            return await reader
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

            return default;

            async Task<Uri> AcquireSasUrl(ICacheEntry entry)
            {
                var storageAccount = await azureResourceService
                    .GetResourceAsync<AzureStorageAccountResource>(storageId.ToString(), false)
                    .ConfigureAwait(false);

                if (storageAccount is null)
                    return null;

                entry.AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(1);

                return await storageAccount
                    .CreateShareFileSasUriAsync(document.ComponentId, $".output/{document.Id}", ShareFileSasPermissions.Read, entry.AbsoluteExpiration.Value.AddMinutes(5))
                    .ConfigureAwait(false);
            }
        }
    }
}
