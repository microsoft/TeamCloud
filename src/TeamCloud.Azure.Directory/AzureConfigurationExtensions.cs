/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TeamCloud.Azure.Directory;

public static class AzureConfigurationExtensions
{
    public static IAzureConfiguration AddDirectory(this IAzureConfiguration azureConfiguration)
    {
        if (azureConfiguration is null)
            throw new ArgumentNullException(nameof(azureConfiguration));

        azureConfiguration.Services
            .TryAddSingleton<IAzureDirectoryService, AzureDirectoryService>();

        return azureConfiguration;
    }
}
