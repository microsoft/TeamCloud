/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.WindowsAzure.Storage;

namespace TeamCloud.Azure.Deployment
{
    public static class Extensions
    {
        private static readonly PropertyInfo IsDevStoreAccountProperty = typeof(CloudStorageAccount).GetProperty("IsDevStoreAccount", BindingFlags.Instance | BindingFlags.NonPublic);

        public static IAzureConfiguration AddDeployment(this IAzureConfiguration azureConfiguration)
        {
            if (azureConfiguration is null)
                throw new ArgumentNullException(nameof(azureConfiguration));

            azureConfiguration.Services
                .TryAddSingleton<IAzureDeploymentService, AzureDeploymentService>();

            return azureConfiguration;
        }

        public static void SetDeploymentArtifactsProvider<T>(this IAzureConfiguration azureConfiguration)
            where T : class, IAzureDeploymentArtifactsProvider
            => (azureConfiguration ?? throw new ArgumentNullException(nameof(azureConfiguration)))
            .AddDeployment().Services.AddSingleton<IAzureDeploymentArtifactsProvider, T>();

        public static void SetDeploymentArtifactsProvider(this IAzureConfiguration azureConfiguration, Func<IServiceProvider, IAzureDeploymentArtifactsProvider> implementationInstance)
            => (azureConfiguration ?? throw new ArgumentNullException(nameof(azureConfiguration)))
            .AddDeployment().Services.AddSingleton(implementationInstance);

        public static void SetDeploymentTokenProvider<T>(this IAzureConfiguration azureConfiguration)
            where T : class, IAzureDeploymentTokenProvider
            => (azureConfiguration ?? throw new ArgumentNullException(nameof(azureConfiguration)))
            .AddDeployment().Services.AddSingleton<IAzureDeploymentTokenProvider, T>();

        public static void SetDeploymentTokenProvider(this IAzureConfiguration azureConfiguration, Func<IServiceProvider, IAzureDeploymentTokenProvider> implementationInstance)
            => (azureConfiguration ?? throw new ArgumentNullException(nameof(azureConfiguration)))
            .AddDeployment().Services.AddSingleton(implementationInstance);

        internal static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> collection)
            => new Dictionary<TKey, TValue>(collection);

        public static bool IsDevelopmentStorageConnectionString(this string value)
            => CloudStorageAccount.TryParse(value, out var account) && account.IsDevelopmentStorage();

        public static bool IsDevelopmentStorage(this CloudStorageAccount account)
            => (bool)IsDevStoreAccountProperty.GetValue(account);

        internal static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
            => new ReadOnlyDictionary<TKey, TValue>(dictionary);

        public static async Task<IReadOnlyDictionary<string, object>> WaitAndGetOutputAsync(this IAzureDeployment azureDeployment, bool throwOnError = false, bool cleanUp = false)
        {
            if (azureDeployment is null)
                throw new ArgumentNullException(nameof(azureDeployment));

            var deploymentOutput = default(IReadOnlyDictionary<string, object>);

            var deploymentState = await azureDeployment
                .WaitAsync(throwOnError: throwOnError)
                .ConfigureAwait(false);

            if (deploymentState == AzureDeploymentState.Succeeded)
            {
                deploymentOutput = await azureDeployment
                    .GetOutputAsync()
                    .ConfigureAwait(false);
            }

            if (cleanUp)
            {
                await azureDeployment
                    .DeleteAsync()
                    .ConfigureAwait(false);
            }

            return deploymentOutput;
        }
    }
}
