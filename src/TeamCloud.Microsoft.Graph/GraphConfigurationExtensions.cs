/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TeamCloud.Microsoft.Graph;

public static class GraphConfigurationExtensions
{
    public static IServiceCollection AddTeamCloudGraph(this IServiceCollection services)
    {
        if (services is null)
            throw new ArgumentNullException(nameof(services));

        services
            .TryAddSingleton<IGraphService, GraphService>();

        return services;
    }
}
