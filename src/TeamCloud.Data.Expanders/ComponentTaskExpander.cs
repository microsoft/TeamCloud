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
using TeamCloud.Model.Common;
using TeamCloud.Audit;
using System.Text;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Azure.Directory;
using Microsoft.ApplicationInsights;

namespace TeamCloud.Data.Expanders
{
    public sealed class ComponentTaskExpander : DocumentExpander,
        IDocumentExpander<ComponentTask>
    {
        private const string Seperator = "\r\n-----\r\n\r\n";

        private readonly IProjectRepository projectRepository;
        private readonly IAzureResourceService azureResourceService;
        private readonly IAzureDirectoryService azureDirectoryService;
        private readonly ICommandAuditReader commandAuditReader;
        private readonly IMemoryCache cache;

        public ComponentTaskExpander(IProjectRepository projectRepository, IAzureResourceService azureResourceService, IAzureDirectoryService azureDirectoryService, ICommandAuditReader commandAuditReader, IMemoryCache cache, TelemetryClient telemetryClient) : base(true, telemetryClient)
        {
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
            this.azureDirectoryService = azureDirectoryService ?? throw new ArgumentNullException(nameof(azureDirectoryService));
            this.commandAuditReader = commandAuditReader ?? throw new ArgumentNullException(nameof(commandAuditReader));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task ExpandAsync(ComponentTask document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            if (string.IsNullOrEmpty(document.Output))
            {
                var results = await Task.WhenAll(

                    GetAuditAsync(document),
                    GetEventsAsync(document),
                    GetOutputAsync(document)

                ).ConfigureAwait(false);

                if (results.Any(result => !string.IsNullOrEmpty(result)))
                {
                    var output = string.Join(Seperator, results.Where(result => !string.IsNullOrEmpty(result)));

                    // do some empty line trimming (left & right)
                    output = Regex.Replace(output, @"^([\s])*", string.Empty, RegexOptions.Singleline);
                    output = Regex.Replace(output, @"([\s])*$", string.Empty, RegexOptions.Singleline);

                    document.Output = output;
                }
            }
        }

        private async Task<string> GetEventsAsync(ComponentTask document)
        {
            if (document.TaskState.IsActive() && AzureResourceIdentifier.TryParse(document.ResourceId, out var resourceId))
            {
                try
                {
                    var containerGroup = await azureResourceService
                        .GetResourceAsync<AzureContainerGroupResource>(resourceId.ToString())
                        .ConfigureAwait(false);

                    if (containerGroup != null)
                    {
                        return await containerGroup
                            .GetEventContentAsync("runner")
                            .ConfigureAwait(false);
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
            var output = default(string);

            var outputUrl = await cache
                .GetOrCreateAsync($"{GetType()}|{document.GetType()}|{document.Id}", GetOutputUrlAsync)
                .ConfigureAwait(false);

            if (outputUrl != null)
            {
                if (document.TaskState.IsActive())
                {
                    output = await GetOutputLogAsync(null)
                        .ConfigureAwait(false);
                }
                else
                {
                    output = await cache
                        .GetOrCreateAsync(outputUrl, GetOutputLogAsync)
                        .ConfigureAwait(false);
                }
            }

            return output;

            async Task<string> GetOutputLogAsync(ICacheEntry entry)
            {
                try
                {
                    using var stream = await outputUrl.ToString()
                        .WithHeader("responsecontent-disposition", "file; attachment")
                        .WithHeader("responsecontent-type", "binary")
                        .GetStreamAsync()
                        .ConfigureAwait(false);

                    if ((stream?.Length ?? 0) > 0)
                    {
                        using var reader = new StreamReader(stream);

                        if (entry != null)
                        {
                            entry.AbsoluteExpiration = DateTime.UtcNow.AddDays(1);

                            entry.Value = await reader
                                .ReadToEndAsync()
                                .ConfigureAwait(false);

                            // add entry to cache if not null
                            if (entry.Value != null) entry.Dispose();
                        }
                        else
                        {

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

                return entry?.Value as string;
            }

            async Task<Uri> GetOutputUrlAsync(ICacheEntry entry)
            {
                try
                {
                    var project = await projectRepository
                        .GetAsync(document.Organization, document.ProjectId)
                        .ConfigureAwait(false);

                    if (AzureResourceIdentifier.TryParse(project?.StorageId, out var storageId))
                    {
                        var storageAccount = await azureResourceService
                            .GetResourceAsync<AzureStorageAccountResource>(storageId.ToString(), false)
                            .ConfigureAwait(false);

                        if (storageAccount != null)
                        {
                            entry.AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(1);

                            entry.Value = await storageAccount // create a shared access token with an additional expiration offset to avoid time sync issues when fetching the output
                                .CreateShareFileSasUriAsync(document.ComponentId, $".output/{document.Id}", ShareFileSasPermissions.Read, entry.AbsoluteExpiration.Value.AddMinutes(5))
                                .ConfigureAwait(false);

                            // add entry to cache if not null
                            if (entry.Value != null) entry.Dispose();
                        }
                    }
                }
                catch
                {
                    // swallow
                }

                return entry?.Value as Uri;
            }
        }

        private async Task<string> GetAuditAsync(ComponentTask document)
        {
            var audit = await commandAuditReader
                .GetAsync(Guid.Parse(document.Organization), Guid.Parse(document.Id))
                .ConfigureAwait(false);

            if (audit != null)
            {
                var username = await azureDirectoryService
                    .GetDisplayNameAsync(audit.UserId)
                    .ConfigureAwait(false);

                var output = new StringBuilder();

                output.AppendLine($"User:       {username}");
                output.AppendLine($"Created:    {audit.Created}");

                if (audit.RuntimeStatus.IsActive())
                {
                    output.AppendLine($"Updated:    {audit.Updated}");
                }
                else
                {
                    output.AppendLine($"Finished:   {audit.Updated}");
                }

                if (!string.IsNullOrEmpty(audit.Errors))
                {
                    output.AppendLine(Seperator);
                    output.AppendLine(audit.Errors);
                }

                return output.ToString();
            }

            return default;
        }
    }
}
