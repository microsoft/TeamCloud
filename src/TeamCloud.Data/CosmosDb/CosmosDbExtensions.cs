using System;
using System.Threading;
using System.Threading.Tasks;

namespace TeamCloud.Data.CosmosDb
{
    internal static class CosmosDbExtensions
    {
        private static readonly SemaphoreSlim lazyInitSemaphore = new SemaphoreSlim(1, 1);

        internal static T Initialize<T>(this Lazy<T> lazy, Action<T> initialize)
        {
            if (lazy is null)
                throw new ArgumentNullException(nameof(lazy));

            if (initialize is null)
                throw new ArgumentNullException(nameof(initialize));

            lazyInitSemaphore.Wait();

            try
            {
                if (!lazy.IsValueCreated)
                    initialize(lazy.Value);

                return lazy.Value;
            }
            finally
            {
                lazyInitSemaphore.Release();
            }
        }

        internal static async Task<T> InitializeAsync<T>(this Lazy<T> lazy, Func<T, Task> initialize)
        {
            if (lazy is null)
                throw new ArgumentNullException(nameof(lazy));

            if (initialize is null)
                throw new ArgumentNullException(nameof(initialize));

            await lazyInitSemaphore
                .WaitAsync()
                .ConfigureAwait(false);

            try
            {
                if (!lazy.IsValueCreated)
                    await initialize(lazy.Value).ConfigureAwait(false);

                return lazy.Value;
            }
            finally
            {
                lazyInitSemaphore.Release();
            }
        }
    }
}
