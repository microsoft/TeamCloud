/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TeamCloud.Azure.Resources;
using TeamCloud.Azure.Resources.Typed;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters.Diagnostics
{
    public sealed class AdapterInitializationLoggerFactory : IAdapterInitializationLoggerFactory
    {
        private readonly IProjectRepository projectRepository;
        private readonly IAzureResourceService azureResourceService;

        public AdapterInitializationLoggerFactory(IProjectRepository projectRepository, IAzureResourceService azureResourceService)
        {
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        public async Task<ILogger> CreateLoggerAsync(ComponentTask componentTask, ILogger logger)
        {
            if (componentTask is null)
                throw new ArgumentNullException(nameof(componentTask));

            logger ??= NullLogger.Instance;

            try
            {
                var project = await projectRepository
                    .GetAsync(componentTask.Organization, componentTask.ProjectId)
                    .ConfigureAwait(false);

                if (AzureResourceIdentifier.TryParse(project?.StorageId, out var storageId))
                {
                    var storageAccount = await azureResourceService
                        .GetResourceAsync<AzureStorageAccountResource>(storageId.ToString(), false)
                        .ConfigureAwait(false);

                    if (storageAccount is not null)
                    {
                        var shareClient = await storageAccount
                            .CreateShareClientAsync(componentTask.ComponentId)
                            .ConfigureAwait(false);

                        var directoryClient = shareClient
                            .GetDirectoryClient(".output");

                        await directoryClient
                            .CreateIfNotExistsAsync()
                            .ConfigureAwait(false);

                        var fileClient = directoryClient
                            .GetFileClient($"{componentTask.Id}");

                        var fileStream = await fileClient
                            .OpenWriteAsync(true, 0)
                            .ConfigureAwait(false);

                        return new AdapterInitializationLogger(logger, fileStream);
                    }
                }
            }
            catch
            {
                // swallow
            }

            return logger;
        }
    }
}
