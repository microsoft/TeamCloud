using System;
using Microsoft.Azure.Cosmos;

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

        public T Repository { get; }

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
