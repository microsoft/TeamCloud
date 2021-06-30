/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;

namespace TeamCloud.Secrets
{
    public interface ISecretsStore
    {
        Task<string> GetSecretAsync(string key);

        Task<T> GetSecretAsync<T>(string key)
            where T : class, new();

        Task<string> SetSecretAsync(string key, string secret);

        Task<T> SetSecretAsync<T>(string key, T secret)
            where T : class, new();
    }
}
