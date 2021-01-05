using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Files.Shares;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using TeamCloud.Azure.Resources.Utilities;

namespace TeamCloud.Azure.Resources.Typed
{
    public sealed class AzureStorageAccountResource : AzureTypedResource
    {
        private readonly AsyncLazy<IStorageAccount> storageInstance;

        internal AzureStorageAccountResource(string resourceId) : base("Microsoft.Storage/storageAccounts", resourceId)
        {
            storageInstance = new AsyncLazy<IStorageAccount>(() => GetStorageAsync());
        }

        private async Task<IStorageAccount> GetStorageAsync()
        {
            var session = await AzureResourceService.AzureSessionService
                .CreateSessionAsync(ResourceId.SubscriptionId)
                .ConfigureAwait(false);

            return await session.StorageAccounts
                .GetByIdAsync(ResourceId.ToString())
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<string>> GetKeysAsync()
        {
            var storage = await storageInstance
                .ConfigureAwait(false);

            var storageKeys = await storage
                .GetKeysAsync()
                .ConfigureAwait(false);

            return storageKeys
                .Select(k => k.Value);
        }

        public async Task<string> GetConnectionStringAsync()
        {
            var storage = await storageInstance
                .ConfigureAwait(false);

            var storageKeys = await storage
                .GetKeysAsync()
                .ConfigureAwait(false);

            var storageCredentials = new StorageCredentials(storage.Name, storageKeys.First().Value);

            return new CloudStorageAccount(storageCredentials, true).ToString(true);
        }

        public async Task<ShareClient> CreateShareClientAsync(string shareName, ShareClientOptions options = null)
        {
            var connectionString = await GetConnectionStringAsync().ConfigureAwait(false);

            return new ShareClient(connectionString, shareName, options);
        }
    }
}
