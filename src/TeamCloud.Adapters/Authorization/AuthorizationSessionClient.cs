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

public sealed class AuthorizationSessionClient : IAuthorizationSessionClient
{
    public const string TableName = "Adapters";

    private readonly ITableService tableService;
    private readonly IAuthorizationSessionOptions options;

    public AuthorizationSessionClient(ITableService tableService, IAuthorizationSessionOptions options)
    {
        this.tableService = tableService ?? throw new ArgumentNullException(nameof(tableService));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<TAuthorizationSession> GetAsync<TAuthorizationSession>(DeploymentScope deploymentScope)
        where TAuthorizationSession : AuthorizationSession, new()
    {
        if (deploymentScope is null)
            throw new ArgumentNullException(nameof(deploymentScope));

        var client = await tableService
            .GetTableClientAsync(options.ConnectionString, TableName)
            .ConfigureAwait(false);

        var rowKey = AuthorizationEntity.GetEntityId(deploymentScope);
        var partitionKey = string.Join(",", typeof(TAuthorizationSession).AssemblyQualifiedName.Split(',').Take(2));

        try
        {
            var response = await client
                .GetEntityAsync<TAuthorizationSession>(partitionKey, rowKey)
                .ConfigureAwait(false);

            var session = response.Value;

            if (session?.Active ?? false)
            {
                return session;
            }
            else if (session is not null)
            {
                await client
                    .DeleteEntityAsync(session.PartitionKey, session.RowKey)
                    .ConfigureAwait(false);
            }
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // doesn't exist
            return null;
        }

        return null;
    }

    public async Task<TAuthorizationSession> SetAsync<TAuthorizationSession>(TAuthorizationSession authorizationSession)
        where TAuthorizationSession : AuthorizationSession, new()
    {
        if (authorizationSession is null)
            throw new ArgumentNullException(nameof(authorizationSession));

        var client = await tableService
            .GetTableClientAsync(options.ConnectionString, TableName)
            .ConfigureAwait(false);

        await client
            .UpsertEntityAsync(authorizationSession, TableUpdateMode.Replace)
            .ConfigureAwait(false);

        return authorizationSession;
    }
}
