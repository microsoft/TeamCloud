/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Nito.AsyncEx;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters.Authorization
{

    public sealed class AuthorizationSessionClient : IAuthorizationSessionClient
    {
        public const string TableName = "Adapters";

        private readonly IAuthorizationSessionOptions options;
        private readonly AsyncLazy<CloudTable> tableInstance;

        public AuthorizationSessionClient(IAuthorizationSessionOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            tableInstance = new AsyncLazy<CloudTable>(async () =>
            {
                var table = CloudStorageAccount
                    .Parse(this.options.ConnectionString)
                    .CreateCloudTableClient()
                    .GetTableReference(TableName);

                await table
                    .CreateIfNotExistsAsync()
                    .ConfigureAwait(false);

                return table;
            });
        }

        public async Task<TAuthorizationSession> GetAsync<TAuthorizationSession>(DeploymentScope deploymentScope)
            where TAuthorizationSession : AuthorizationSession
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            var table = await tableInstance.ConfigureAwait(false);

            var rowKey = AuthorizationEntity.GetEntityId(deploymentScope);
            var partitionKey = string.Join(",", typeof(TAuthorizationSession).AssemblyQualifiedName.Split(',').Take(2));

            var response = await table
                .ExecuteAsync(TableOperation.Retrieve<TAuthorizationSession>(partitionKey, rowKey))
                .ConfigureAwait(false);

            var session = response.Result as TAuthorizationSession;

            if (session?.Active ?? false)
            {
                return session;
            }
            else if (session != null)
            {
                await table
                    .ExecuteAsync(TableOperation.Delete(session))
                    .ConfigureAwait(false);
            }

            return null;
        }

        public async Task<TAuthorizationSession> SetAsync<TAuthorizationSession>(TAuthorizationSession authorizationSession)
            where TAuthorizationSession : AuthorizationSession
        {
            if (authorizationSession is null)
                throw new ArgumentNullException(nameof(authorizationSession));

            var table = await tableInstance.ConfigureAwait(false);

            var response = await table
                .ExecuteAsync(TableOperation.InsertOrReplace(authorizationSession))
                .ConfigureAwait(false);

            return response.Result as TAuthorizationSession;
        }
    }
}
