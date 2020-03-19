/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Reflection;
using Flurl.Http.Configuration;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using static Microsoft.Azure.Management.ResourceManager.Fluent.Core.RestClient.RestClientBuilder;

namespace TeamCloud.Azure
{
    public static class Extensions
    {

        public static IServiceCollection AddTeamCloudAzure(this IServiceCollection services, Action<IAzureConfiguration> configuration)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            services
                .TryAddSingleton<IAzureSessionService, AzureSessionService>();

            configuration.Invoke(new AzureConfiguration(services));

            return services;
        }

        public static string GetEndpointUrl(this AzureEnvironment azureEnvironment, AzureEndpoint azureEndpoint)
        {
            if (azureEnvironment is null)
                throw new ArgumentNullException(nameof(azureEnvironment));

            return azureEndpoint switch
            {
                AzureEndpoint.ResourceManagerEndpoint => azureEnvironment.ResourceManagerEndpoint,
                AzureEndpoint.GraphEndpoint => azureEnvironment.GraphEndpoint,
                _ => throw new NotSupportedException($"The Azure endpoint {azureEndpoint} is not supported.")
            };
        }

        internal static T WithDelegatingHandler<T>(this IAzureConfigurable<T> azureConfigurable, IHttpClientFactory httpClientFactory) where T : IAzureConfigurable<T>
        {
            var messageHandler = httpClientFactory?
                .CreateMessageHandler() as DelegatingHandler;

            if (messageHandler != null)
                azureConfigurable = azureConfigurable.WithDelegatingHandler(messageHandler);

            return (T)azureConfigurable;
        }

        internal static IBuildable WithDelegatingHandler(this IBuildable builder, IHttpClientFactory httpClientFactory)
        {
            var messageHandler = httpClientFactory?
                .CreateMessageHandler() as DelegatingHandler;

            if (messageHandler != null)
                builder = builder.WithDelegatingHandler(messageHandler);

            return builder;
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Swallows exception if GetProperty fails.")]
        internal static bool TryGetProperty(this Type type, string name, out PropertyInfo propertyInfo)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            try
            {
                propertyInfo = type.GetProperty(name);
            }
            catch
            {
                propertyInfo = null;
            }

            return propertyInfo != null;
        }
    }
}
