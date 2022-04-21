/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */


using System.Threading.Tasks;
using Azure;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Http;
using TeamCloud.Serialization;

namespace TeamCloud.Azure.KeyVault;

public static class SecretClientExtensions
{
    public static async Task<T> SetSecretAsync<T>(this SecretClient client, string name, T value)
        where T : class, new()
    {
        if (value is null)
        {
            await client.StartDeleteSecretAsync(name)
                .ConfigureAwait(false);

            return value;
        }
        else
        {
            var response = await client.SetSecretAsync(name, TeamCloudSerialize.SerializeObject(value))
                .ConfigureAwait(false);

            return TeamCloudSerialize.DeserializeObject<T>(response.Value.Value);
        }
    }

    public static async Task<T> GetSecretAsync<T>(this SecretClient client, string name)
        where T : class, new()
    {
        try
        {
            var secret = await client.GetSecretAsync(name)
                .ConfigureAwait(false);

            return string.IsNullOrEmpty(secret?.Value.Value)
                ? default
                : TeamCloudSerialize.DeserializeObject<T>(secret.Value.Value);
        }
        catch (RequestFailedException exc) when (exc.Status == StatusCodes.Status404NotFound)
        {
            return null;
        }
    }
}
