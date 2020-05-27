/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data.Core;
using Xunit;

namespace TeamCloud.Data.CosmosDb.Core
{
    public abstract class CosmosDbRepositoryTests<T>
        where T : class, ICosmosDbRepository
    {
        private CosmosDbRepositoryFixture fixture;

        protected CosmosDbRepositoryTests(T repository)
        {
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        protected CosmosDbRepositoryTests(CosmosDbRepositoryFixture fixture)
        {
            this.fixture = fixture;
        }

        protected T Repository { get; }

        protected void AssertContainerDocumentMetadata(IContainerDocument containerDocument)
        {
            Assert.NotNull(containerDocument);
            Assert.NotNull(containerDocument.ETag);
            Assert.NotNull(containerDocument.Timestamp);
        }
    }
}
