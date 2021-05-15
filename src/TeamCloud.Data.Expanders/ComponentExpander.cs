/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.IO;
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

        public ComponentExpander(IProjectRepository projectRepository, IAzureResourceService azureResourceService) : base(true)
        {
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        public async Task<Component> ExpandAsync(Component document)
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
                    try
                    {
                        var storageAccount = await azureResourceService
                            .GetResourceAsync<AzureStorageAccountResource>(storageId.ToString(), false)
                            .ConfigureAwait(false);

                        if (storageAccount != null)
                        {
                            var shareClient = await storageAccount
                                .CreateShareClientAsync(document.Id)
                                .ConfigureAwait(false);

                            var fileClient = shareClient
                                .GetRootDirectoryClient()
                                .GetFileClient($"value.json");

                            if (await fileClient.ExistsAsync().ConfigureAwait(false))
                            {
                                using var reader = new StreamReader(await fileClient.OpenReadAsync().ConfigureAwait(false));

                                document.ValueJson = await reader.ReadToEndAsync().ConfigureAwait(false);
                            }
                        }
                    }
                    catch
                    {
                        // swallow
                    }
                }
            }

            return document;
        }
    }
}
