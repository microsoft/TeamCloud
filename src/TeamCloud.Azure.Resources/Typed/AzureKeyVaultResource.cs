/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Management.KeyVault.Fluent;
using Microsoft.Azure.Management.KeyVault.Fluent.Models;
using Newtonsoft.Json;
using TeamCloud.Azure.Resources.Utilities;

namespace TeamCloud.Azure.Resources.Typed
{
    public sealed class AzureKeyVaultResource : AzureTypedResource
    {
        private readonly AsyncLazy<IVault> vaultInstance;

        internal AzureKeyVaultResource(string resourceId) : base("Microsoft.KeyVault/vaults", resourceId)
        {
            vaultInstance = new AsyncLazy<IVault>(() => GetVaultAsync());
        }

        private Task<IVault> GetVaultAsync()
        {
            var session = AzureResourceService.AzureSessionService
                .CreateSession(ResourceId.SubscriptionId);

            return session.Vaults
                .GetByIdAsync(ResourceId.ToString());
        }

        public async Task SetAllSecretPermissionsAsync(Guid userObjectId)
        {
            try
            {
                var vault = await vaultInstance
                    .ConfigureAwait(false);

                await vault.Update()
                    .DefineAccessPolicy()
                    .ForObjectId(userObjectId.ToString())
                    .AllowSecretAllPermissions()
                    .Attach()
                    .ApplyAsync()
                    .ConfigureAwait(false);
            }
            finally
            {
                vaultInstance.Reset();
            }
        }

        public async Task SetSecretPermissionsAsync(Guid userObjectId, params SecretPermissions[] secretPermissions)
        {
            try
            {
                var vault = await vaultInstance
                    .ConfigureAwait(false);

                await vault.Update()
                    .DefineAccessPolicy()
                    .ForObjectId(userObjectId.ToString())
                    .AllowSecretPermissions(secretPermissions)
                    .Attach()
                    .ApplyAsync()
                    .ConfigureAwait(false);
            }
            finally
            {
                vaultInstance.Reset();
            }
        }

        public async Task<T> SetSecretAsync<T>(string secretName, T secretValue)
            where T : class, new()
        {
            if (secretValue is null)
            {
                await SetSecretAsync(secretName, null)
                    .ConfigureAwait(false);

                return secretValue;
            }
            else
            {
                var secretJson = await SetSecretAsync(secretName, JsonConvert.SerializeObject(secretValue))
                    .ConfigureAwait(false);

                return JsonConvert.DeserializeObject<T>(secretJson);
            }
        }

        public async Task<string> SetSecretAsync(string secretName, string secretValue)
        {
            var vault = await vaultInstance
                .ConfigureAwait(false);

            if (secretValue is null)
            {
                await vault.Client
                    .DeleteSecretAsync(vault.VaultUri, secretName)
                    .ConfigureAwait(false);

                return secretValue;
            }
            else
            {
                var secret = await vault.Client
                    .SetSecretAsync(vault.VaultUri, secretName, secretValue)
                    .ConfigureAwait(false);

                return secret.Value;
            }
        }

        public async Task<T> GetSecretAsync<T>(string secretName)
            where T : class, new()
        {
            var secretJson = await GetSecretAsync(secretName)
                .ConfigureAwait(false);

            return string.IsNullOrEmpty(secretJson)
                ? default(T)
                : JsonConvert.DeserializeObject<T>(secretJson);
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            var vault = await vaultInstance
                .ConfigureAwait(false);

            try
            {
                var secret = await vault.Client
                    .GetSecretAsync(vault.VaultUri, secretName)
                    .ConfigureAwait(false);

                return secret.Value;
            }
            catch (KeyVaultErrorException exc) when (exc.Response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task SetAllCertificatePermissionsAsync(Guid userObjectId)
        {
            try
            {
                var vault = await vaultInstance
                    .ConfigureAwait(false);

                await vault.Update()
                    .DefineAccessPolicy()
                    .ForObjectId(userObjectId.ToString())
                    .AllowCertificateAllPermissions()
                    .Attach()
                    .ApplyAsync()
                    .ConfigureAwait(false);
            }
            finally
            {
                vaultInstance.Reset();
            }
        }

        public async Task SetCertificatePermissionsAsync(Guid userObjectId, params CertificatePermissions[] certificatePermissions)
        {
            try
            {
                var vault = await vaultInstance
                    .ConfigureAwait(false);

                await vault.Update()
                    .DefineAccessPolicy()
                    .ForObjectId(userObjectId.ToString())
                    .AllowCertificatePermissions(certificatePermissions)
                    .Attach()
                    .ApplyAsync()
                    .ConfigureAwait(false);
            }
            finally
            {
                vaultInstance.Reset();
            }
        }

        public async Task SetAllKeyPermissionsAsync(Guid userObjectId)
        {
            try
            {
                var vault = await vaultInstance
                    .ConfigureAwait(false);

                await vault.Update()
                    .DefineAccessPolicy()
                    .ForObjectId(userObjectId.ToString())
                    .AllowKeyAllPermissions()
                    .Attach()
                    .ApplyAsync()
                    .ConfigureAwait(false);
            }
            finally
            {
                vaultInstance.Reset();
            }
        }

        public async Task SetKeyPermissionsAsync(Guid userObjectId, params KeyPermissions[] keyPermissions)
        {
            try
            {
                var vault = await vaultInstance
                    .ConfigureAwait(false);

                await vault.Update()
                    .DefineAccessPolicy()
                    .ForObjectId(userObjectId.ToString())
                    .AllowKeyPermissions(keyPermissions)
                    .Attach()
                    .ApplyAsync()
                    .ConfigureAwait(false);
            }
            finally
            {
                vaultInstance.Reset();
            }
        }
    }
}
