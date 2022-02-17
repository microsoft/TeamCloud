/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Nito.AsyncEx;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters.Authorization;

public sealed class AuthorizationTokenClient : IAuthorizationTokenClient
{
    public const string TableName = "Adapters";

    private readonly IAuthorizationTokenOptions options;

    private readonly AsyncLazy<TableClient> tableClient;

    public AuthorizationTokenClient(IAuthorizationTokenOptions options)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));

        tableClient = new AsyncLazy<TableClient>(async () =>
        {
            var client = new TableClient(this.options.ConnectionString, TableName);

            await client.CreateIfNotExistsAsync().ConfigureAwait(false);

            return client;
        });
    }

    public async Task<TAuthorizationToken> GetAsync<TAuthorizationToken>(DeploymentScope deploymentScope)
        where TAuthorizationToken : AuthorizationToken, new()
    {
        if (deploymentScope is null)
            throw new ArgumentNullException(nameof(deploymentScope));

        var client = await tableClient.ConfigureAwait(false);

        var rowKey = AuthorizationEntity.GetEntityId(deploymentScope);
        var partitionKey = string.Join(",", typeof(TAuthorizationToken).AssemblyQualifiedName.Split(',').Take(2));

        try
        {
            var response = await client
                .GetEntityAsync<TAuthorizationToken>(partitionKey, rowKey)
                .ConfigureAwait(false);

            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // doesn't exist
            return null;
        }
    }

    public async Task<TAuthorizationToken> SetAsync<TAuthorizationToken>(TAuthorizationToken authorizationToken, bool force = false)
        where TAuthorizationToken : AuthorizationToken, new()
    {
        if (authorizationToken is null)
            throw new ArgumentNullException(nameof(authorizationToken));

        var client = await tableClient.ConfigureAwait(false);

        if (authorizationToken.ETag != default && force)
            authorizationToken.ETag = ETag.All;

        var updateMode = authorizationToken.ETag == default || force
            ? TableUpdateMode.Replace
            : TableUpdateMode.Merge;

        await client
            .UpsertEntityAsync(authorizationToken, updateMode)
            .ConfigureAwait(false);

        return authorizationToken;
    }
}
