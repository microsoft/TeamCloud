/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.IO;
using System.Text;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Adapters.Diagnostics;
using TeamCloud.Adapters.Threading;
using TeamCloud.Configuration;

namespace TeamCloud.Adapters
{
    public static class AdapterExtensions
    {
        public static string ToString(this JSchema schema, Formatting formatting)
        {
            if (schema is null)
                throw new ArgumentNullException(nameof(schema));

            var sb = new StringBuilder();

            using var sw = new StringWriter(sb);
            using var jw = new JsonTextWriter(sw) { Formatting = formatting };

            schema.WriteTo(jw);

            return sb.ToString();
        }

        public static IServiceCollection AddTeamCloudAdapterProvider(this IServiceCollection services, Action<IAdapterProviderConfig> configuration)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            services
                .AddTeamCloudOptions();

            services
                .TryAddSingleton<IAdapterInitializationLoggerFactory, AdapterInitializationLoggerFactory>();

            services
                .TryAddSingleton<IDistributedLockManager, BlobStorageDistributedLockManager>();

            services
                .TryAddSingleton<IAuthorizationEndpointsResolver, AuthorizationEndpointsResolver>();

            services
                .TryAddSingleton<IAuthorizationSessionClient, AuthorizationSessionClient>();

            services
                .TryAddSingleton<IAuthorizationTokenClient, AuthorizationTokenClient>();

            services
                .TryAddTransient<IAdapterProvider>(provider => new AdapterProvider(provider));

            var serviceProvider = services.BuildServiceProvider();

            var adapterConfiguration = serviceProvider.GetService<IAdapterProviderConfig>()
                ?? serviceProvider.GetService<IAdapterProvider>() as IAdapterProviderConfig;

            configuration(adapterConfiguration);

            return services;
        }
    }
}
