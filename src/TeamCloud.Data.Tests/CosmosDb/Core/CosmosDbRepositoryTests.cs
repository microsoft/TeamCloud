/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Microsoft.Azure.Cosmos;
using TeamCloud.Model.Data.Core;
using Xunit;

namespace TeamCloud.Data.CosmosDb.Core
{
    public abstract class CosmosDbRepositoryTests<T> : IDisposable
        where T : class, ICosmosDbRepository
    {
        private bool disposedValue;

        protected CosmosDbRepositoryTests(T repository)
        {
            Repository = repository ?? throw new System.ArgumentNullException(nameof(repository));
        }

        ~CosmosDbRepositoryTests()
        {
            Dispose(disposing: false);
        }

        protected T Repository { get; }

        protected void AssertContainerDocumentMetadata(IContainerDocument containerDocument)
        {
            Assert.NotNull(containerDocument);
            Assert.NotNull(containerDocument.ETag);
            Assert.NotNull(containerDocument.Timestamp);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                try
                {
                    using var client = new CosmosClient(Repository.Options.ConnectionString);

                    client
                        .GetContainer(Repository.Options.DatabaseName, Repository.ContainerDocumentType.Name)
                        .DeleteContainerAsync()
                        .Wait();
                }
                catch
                {
                    // swallow
                }
                finally
                {
                    disposedValue = true;
                }
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);

            GC.SuppressFinalize(this);
        }
    }
}
