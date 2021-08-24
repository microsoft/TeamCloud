﻿/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Adapters.Diagnostics;
using TeamCloud.Adapters.Threading;
using TeamCloud.Model.Data;

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

        public static IServiceCollection AddTeamCloudAdapters(this IServiceCollection services, Action<IAdapterConfiguration> configuration)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            services
                .TryAddSingleton<IAdapterInitializationLoggerFactory, AdapterInitializationLoggerFactory>();

            services
                .TryAddSingleton<IDistributedLockManager, BlobStorageDistributedLockManager>();

            services
                .TryAddTransient(provider => AuthorizationSessionOptions.Default);

            services
                .TryAddSingleton<IAuthorizationSessionClient, AuthorizationSessionClient>();

            services
                .TryAddTransient(provider => AuthorizationTokenOptions.Default);

            services
                .TryAddSingleton<IAuthorizationTokenClient, AuthorizationTokenClient>();

            services
                .TryAddTransient<IAdapterProvider>(provider => new AdapterProvider(provider));

            var serviceProvider = services.BuildServiceProvider();

            var adapterConfiguration = serviceProvider.GetService<IAdapterConfiguration>()
                ?? serviceProvider.GetService<IAdapterProvider>() as IAdapterConfiguration;

            configuration(adapterConfiguration);

            return services;
        }
    }
}
