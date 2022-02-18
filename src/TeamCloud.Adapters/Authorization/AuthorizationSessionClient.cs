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

public sealed class AuthorizationSessionClient : IAuthorizationSessionClient
{
    public const string TableName = "Adapters";

    private readonly IAuthorizationSessionOptions options;
    private readonly AsyncLazy<TableClient> tableClient;

    public AuthorizationSessionClient(IAuthorizationSessionOptions options)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));

        tableClient = new AsyncLazy<TableClient>(async () =>
        {
            var client = new TableClient(this.options.ConnectionString, TableName);

            await client.CreateIfNotExistsAsync().ConfigureAwait(false);

            return client;
        });
    }

    public async Task<TAuthorizationSession> GetAsync<TAuthorizationSession>(DeploymentScope deploymentScope)
        where TAuthorizationSession : AuthorizationSession, new()
    {
        if (deploymentScope is null)
            throw new ArgumentNullException(nameof(deploymentScope));

        var client = await tableClient.ConfigureAwait(false);

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

        var client = await tableClient.ConfigureAwait(false);

        await client
            .UpsertEntityAsync(authorizationSession, TableUpdateMode.Replace)
            .ConfigureAwait(false);

        return authorizationSession;
    }
}
