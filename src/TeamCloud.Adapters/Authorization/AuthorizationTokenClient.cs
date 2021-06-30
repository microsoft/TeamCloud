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
    public sealed class AuthorizationTokenClient : IAuthorizationTokenClient
    {
        public const string TableName = "AuthorizationToken";

        private readonly IAuthorizationTokenOptions options;

        private readonly AsyncLazy<CloudTable> tableInstance;

        public AuthorizationTokenClient(IAuthorizationTokenOptions options)
        {
            this.options = options ?? AuthorizationTokenOptions.Default;

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

        public async Task<TAuthorizationToken> GetAsync<TAuthorizationToken>(DeploymentScope deploymentScope)
            where TAuthorizationToken : AuthorizationToken
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            var table = await tableInstance.ConfigureAwait(false);

            var rowKey = AuthorizationEntity.GetEntityId(deploymentScope);
            var partitionKey = string.Join(",", typeof(TAuthorizationToken).AssemblyQualifiedName.Split(',').Take(2));

            var response = await table
                .ExecuteAsync(TableOperation.Retrieve<TAuthorizationToken>(partitionKey, rowKey))
                .ConfigureAwait(false);

            return response.Result as TAuthorizationToken;
        }

        public async Task<TAuthorizationToken> SetAsync<TAuthorizationToken>(TAuthorizationToken authorizationToken, bool force = false)
            where TAuthorizationToken : AuthorizationToken
        {
            if (authorizationToken is null)
                throw new ArgumentNullException(nameof(authorizationToken));

            var table = await tableInstance.ConfigureAwait(false);

            if (authorizationToken.Entity.ETag != null && force)
                authorizationToken.Entity.ETag = "*";

            var tableOperation = authorizationToken.Entity.ETag is null || force
                ? TableOperation.InsertOrReplace(authorizationToken)
                : TableOperation.Replace(authorizationToken);

            var response = await table
                .ExecuteAsync(tableOperation)
                .ConfigureAwait(false);

            return response.Result as TAuthorizationToken;
        }
    }
}
