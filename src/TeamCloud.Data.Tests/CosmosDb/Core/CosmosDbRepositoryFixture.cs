/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Xunit;

namespace TeamCloud.Data.CosmosDb.Core
{
    public sealed class CosmosDbRepositoryFixture : IAsyncLifetime
    {
        public Task InitializeAsync()
            => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            using var client = new CosmosClient(CosmosDbTestOptions.Instance.ConnectionString);

            await client
                .GetDatabase(CosmosDbTestOptions.Instance.DatabaseName)
                .DeleteAsync()
                .ConfigureAwait(false);
        }
    }
}
