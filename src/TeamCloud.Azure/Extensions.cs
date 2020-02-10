/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TeamCloud.Azure
{
    public static class Extensions
    {

        public static IServiceCollection AddTeamCloudAzure(this IServiceCollection services, Action<IAzureConfiguration> configuration)
        {
            services
                .TryAddSingleton<IAzureSessionService, AzureSessionService>();

            configuration.Invoke(new AzureConfiguration(services));

            return services;
        }
    }
}
