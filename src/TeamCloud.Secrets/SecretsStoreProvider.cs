/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;
using TeamCloud.Azure.Resources;
using TeamCloud.Azure.Resources.Typed;
using TeamCloud.Model.Data;

namespace TeamCloud.Secrets
{
    public sealed class SecretsStoreProvider<TSecretsStore> : ISecretsStoreProvider
        where TSecretsStore : ISecretsStore
    {
        private static readonly ConcurrentDictionary<string, AsyncLazy<AzureKeyVaultResource>> keyVaultResourceCache = new ConcurrentDictionary<string, AsyncLazy<AzureKeyVaultResource>>();

        private readonly IServiceProvider serviceProvider;
        private readonly IAzureResourceService azureResourceService;

        internal SecretsStoreProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.azureResourceService = serviceProvider.GetRequiredService<IAzureResourceService>();
        }

        public async Task<ISecretsStore> GetSecretsStoreAsync(Project project)
        {
            if (project is null)
                throw new ArgumentNullException(nameof(project));

            if (string.IsNullOrEmpty(project.SecretsVaultId))
                throw new ArgumentException($"Secrets vault not available.", nameof(project));

            var keyVaultResource = await keyVaultResourceCache.GetOrAdd(project.SecretsVaultId, (key) => new AsyncLazy<AzureKeyVaultResource>(async () =>
            {
                var secretsVault = await azureResourceService
                    .GetResourceAsync<AzureKeyVaultResource>(project.SharedVaultId, true)
                    .ConfigureAwait(false);

                var identity = await azureResourceService.AzureSessionService
                    .GetIdentityAsync()
                    .ConfigureAwait(false);

                await secretsVault
                    .SetAllSecretPermissionsAsync(identity.ObjectId)
                    .ConfigureAwait(false);

                return secretsVault;

            })).ConfigureAwait(false);

            try
            {
                return ActivatorUtilities.CreateInstance<TSecretsStore>(serviceProvider, keyVaultResource);
            }
            catch
            {
                throw;
            }
        }
    }
}
