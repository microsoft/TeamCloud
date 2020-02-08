/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TeamCloud.Http
{

    public static class Extensions
    {
        public static IServiceCollection AddTeamCloudHttp(this IServiceCollection services, Action<GlobalFlurlHttpSettings> configure = null)
        {
            var serviceProvider = services
                .Replace(ServiceDescriptor.Singleton<IHttpClientFactory, TeamCloudHttpClientFactory>())
                .BuildServiceProvider();

            FlurlHttp.Configure(configuration =>
            {
                configuration.HttpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                configure?.Invoke(configuration);
            });

            return services;
        }
    }
}
