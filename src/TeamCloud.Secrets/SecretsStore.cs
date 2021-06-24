/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using TeamCloud.Azure.Resources.Typed;

namespace TeamCloud.Secrets
{
    public sealed class SecretsStore : ISecretsStore
    {
        private readonly AzureKeyVaultResource keyVaultResource;

        internal SecretsStore(AzureKeyVaultResource keyVaultResource)
        {
            this.keyVaultResource = keyVaultResource ?? throw new System.ArgumentNullException(nameof(keyVaultResource));
        }

        public Task<string> GetSecretAsync(string key)
            => keyVaultResource.GetSecretAsync(key);

        public Task<T> GetSecretAsync<T>(string key)
            where T : class, new()
            => keyVaultResource.GetSecretAsync<T>(key);

        public Task<string> SetSecretAsync(string key, string secret)
            => keyVaultResource.SetSecretAsync(key, secret);

        public Task<T> SetSecretAsync<T>(string key, T secret)
            where T : class, new()
            => keyVaultResource.SetSecretAsync<T>(key, secret);

    }
}
