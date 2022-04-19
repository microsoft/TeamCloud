/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Storage.Sas;
using Flurl.Http;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Caching.Memory;
using TeamCloud.Audit;
using TeamCloud.Azure.Resources;
using TeamCloud.Azure.Resources.Typed;
using TeamCloud.Microsoft.Graph;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;

namespace TeamCloud.Data.Expanders;

public sealed class ComponentTaskExpander : DocumentExpander,
    IDocumentExpander<ComponentTask>
{
    private const string Seperator = "\r\n-----\r\n\r\n";

    private readonly IProjectRepository projectRepository;
    private readonly IAzureResourceService azureResourceService;
    private readonly IGraphService graphService;
    private readonly ICommandAuditReader commandAuditReader;
    private readonly IMemoryCache cache;

    public ComponentTaskExpander(IProjectRepository projectRepository, IAzureResourceService azureResourceService, IGraphService graphService, ICommandAuditReader commandAuditReader, IMemoryCache cache, TelemetryClient telemetryClient) : base(true, telemetryClient)
    {
        this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        this.graphService = graphService ?? throw new ArgumentNullException(nameof(graphService));
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
        var output = default(string);

        if (document.TaskState.IsActive() && AzureResourceIdentifier.TryParse(document.ResourceId, out var resourceId))
        {
            try
            {
                var containerGroup = await azureResourceService
                    .GetResourceAsync<AzureContainerGroupResource>(resourceId.ToString())
                    .ConfigureAwait(false);

                if (containerGroup is not null)
                {
                    output = await containerGroup
                        .GetEventContentAsync(document.Id)
                        .ConfigureAwait(false);
                }
            }
            catch
            {
                // swallow
            }
        }

        return output;
    }

    private async Task<string> GetOutputAsync(ComponentTask document)
    {
        var output = default(string);
        var outputKey = $"{GetType()}|{document.GetType()}|{document.Id}";

        var outputUrl = await cache
            .GetOrCreateAsync(outputKey, GetOutputUrlAsync)
            .ConfigureAwait(false);

        if (outputUrl is null)
        {
            cache.Remove(outputKey);
        }
        else
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

                if (string.IsNullOrEmpty(output))
                {
                    cache.Remove(outputUrl);
                }
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

                    if (entry is not null)
                    {
                        entry.AbsoluteExpiration = DateTime.UtcNow.AddDays(1);

                        entry.Value = await reader
                            .ReadToEndAsync()
                            .ConfigureAwait(false);
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

                    if (storageAccount is not null)
                    {
                        entry.AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(1);

                        entry.Value = await storageAccount // create a shared access token with an additional expiration offset to avoid time sync issues when fetching the output
                            .CreateShareFileSasUriAsync(document.ComponentId, $".output/{document.Id}", ShareFileSasPermissions.Read, entry.AbsoluteExpiration.Value.AddMinutes(5))
                            .ConfigureAwait(false);
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

        if (audit is not null)
        {
            var username = await graphService
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
