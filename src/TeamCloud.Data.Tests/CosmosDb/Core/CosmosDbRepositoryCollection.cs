/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Xunit;

namespace TeamCloud.Data.CosmosDb.Core
{
    [CollectionDefinition(nameof(CosmosDbRepositoryCollection))]
    public sealed class CosmosDbRepositoryCollection : ICollectionFixture<CosmosDbRepositoryFixture>
    { }
}
