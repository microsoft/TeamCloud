/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TeamCloud.Azure.Resources;
using TeamCloud.Azure.Resources.Typed;
using TeamCloud.Model.Data;

namespace TeamCloud.Data.Expanders
{
    public sealed class ComponentTaskExpander : DocumentExpander,
        IDocumentExpander<ComponentTask>
    {
        private readonly IProjectRepository projectRepository;
        private readonly IAzureResourceService azureResourceService;

        public ComponentTaskExpander(IProjectRepository projectRepository, IAzureResourceService azureResourceService)
        {
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        public async Task<ComponentTask> ExpandAsync(ComponentTask document)
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

            return document;
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
                try
                {
                    var storageAccount = await azureResourceService
                    .GetResourceAsync<AzureStorageAccountResource>(storageId.ToString(), false)
                    .ConfigureAwait(false);

                    if (storageAccount != null)
                    {
                        var shareClient = await storageAccount
                            .CreateShareClientAsync(document.ComponentId)
                            .ConfigureAwait(false);

                        var dirClient = shareClient
                            .GetDirectoryClient(".output");

                        await dirClient
                            .CreateIfNotExistsAsync()
                            .ConfigureAwait(false);

                        var fileClient = dirClient
                            .GetFileClient($"{document.Id}");

                        if (await fileClient.ExistsAsync().ConfigureAwait(false))
                        {
                            using var reader = new StreamReader(await fileClient.OpenReadAsync().ConfigureAwait(false));

                            return await reader.ReadToEndAsync().ConfigureAwait(false);
                        }
                    }
                }
                catch
                {
                    // swallow
                }
            }

            return default;
        }
    }
}
