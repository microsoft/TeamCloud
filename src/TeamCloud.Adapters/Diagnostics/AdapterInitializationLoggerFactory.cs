/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TeamCloud.Azure;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters.Diagnostics;

public sealed class AdapterInitializationLoggerFactory : IAdapterInitializationLoggerFactory
{
    private readonly IProjectRepository projectRepository;
    private readonly IAzureService azure;

    public AdapterInitializationLoggerFactory(IProjectRepository projectRepository, IAzureService azure)
    {
        this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        this.azure = azure ?? throw new ArgumentNullException(nameof(azure));
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

            if (!string.IsNullOrEmpty(project?.StorageId) && await azure.ExistsAsync(project.StorageId).ConfigureAwait(false))
            {
                var fileClient = await azure.Storage.FileShares
                    .GetShareFileClientAsync(project.StorageId, componentTask.ComponentId, ".output", $"{componentTask.Id}", ensureDirectroyExists: true)
                    .ConfigureAwait(false);

                var fileStream = await fileClient
                    .OpenWriteAsync(true, 0)
                    .ConfigureAwait(false);

                return new AdapterInitializationLogger(logger, fileStream);
            }
        }
        catch
        {
            // swallow
        }

        return logger;
    }
}
