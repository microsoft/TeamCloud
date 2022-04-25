/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using TeamCloud.Azure.Storage;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters.Authorization;

public sealed class AuthorizationTokenClient : IAuthorizationTokenClient
{
    public const string TableName = "Adapters";

    private readonly ITableService tableService;
    private readonly IAuthorizationTokenOptions options;

    public AuthorizationTokenClient(ITableService tableService, IAuthorizationTokenOptions options)
    {
        this.tableService = tableService ?? throw new ArgumentNullException(nameof(tableService));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<TAuthorizationToken> GetAsync<TAuthorizationToken>(DeploymentScope deploymentScope)
        where TAuthorizationToken : AuthorizationToken, new()
    {
        if (deploymentScope is null)
            throw new ArgumentNullException(nameof(deploymentScope));

        var client = await tableService
            .GetTableClientAsync(options.ConnectionString, TableName)
            .ConfigureAwait(false);

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

        var client = await tableService
            .GetTableClientAsync(options.ConnectionString, TableName)
            .ConfigureAwait(false);

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
