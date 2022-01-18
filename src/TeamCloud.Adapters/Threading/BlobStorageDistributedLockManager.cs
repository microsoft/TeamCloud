/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Nito.AsyncEx;

namespace TeamCloud.Adapters.Threading
{

#pragma warning disable CS0618 // Type or member is obsolete

    // IDistributedLockManager is marked as obsolete, because it's not ready for "prime time"
    // however; it is used to managed singleton function execution within the functions fx !!!

    public sealed class BlobStorageDistributedLockManager : IDistributedLockManager
    {

#pragma warning restore CS0608

        private const string OWNERID_METADATA = "OwnerId";
        private const string CONTAINER_NAME = "distributed-locks";

        private readonly ConcurrentDictionary<string, CloudBlobDirectory> lockDirectoryMap = new ConcurrentDictionary<string, CloudBlobDirectory>(StringComparer.OrdinalIgnoreCase);

        private readonly IBlobStorageDistributeLockOptions options;
        private readonly AsyncLazy<CloudBlobContainer> containerInstance;

        public BlobStorageDistributedLockManager(IBlobStorageDistributeLockOptions options = null)
        {
            this.options = options ?? BlobStorageDistributeeLockOptions.Default;

            containerInstance = new AsyncLazy<CloudBlobContainer>(async () =>
            {
                var container = CloudStorageAccount
                    .Parse(this.options.ConnectionString)
                    .CreateCloudBlobClient()
                    .GetContainerReference(CONTAINER_NAME);

                await container
                    .CreateIfNotExistsAsync()
                    .ConfigureAwait(false);

                return container;
            });
        }

        private async Task<CloudBlockBlob> GetLockBlobAsync(string account, string lockId)
        {
            if (string.IsNullOrWhiteSpace(lockId))
                throw new ArgumentException($"'{nameof(lockId)}' cannot be null or whitespace.", nameof(lockId));

            account ??= string.Empty;

            if (!lockDirectoryMap.TryGetValue(account, out var directory))
            {
                var container = await containerInstance.ConfigureAwait(false);

                lockDirectoryMap[account] = directory = container.GetDirectoryReference(account);
            }

            return directory.GetBlockBlobReference(lockId);
        }

        public async Task<string> GetLockOwnerAsync(string account, string lockId, CancellationToken cancellationToken)
        {
            var lockBlob = await GetLockBlobAsync(account, lockId)
                .ConfigureAwait(false);

            try
            {
                await lockBlob
                    .FetchAttributesAsync(accessCondition: null, options: null, operationContext: null, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (StorageException exc) when ((exc.RequestInformation?.HttpStatusCode).GetValueOrDefault() == 404)
            {
                // swallow - blob no longer exists
            }

            if ((lockBlob.Properties.LeaseState != LeaseState.Available || lockBlob.Properties.LeaseStatus != LeaseStatus.Unlocked) && lockBlob.Metadata.TryGetValue(OWNERID_METADATA, out var owner))
            {
                return owner;
            }

            return null;
        }

        public Task ReleaseLockAsync(IDistributedLock lockHandle, CancellationToken cancellationToken)
        {
            if (lockHandle is null)
                throw new ArgumentNullException(nameof(lockHandle));

            var lockHandleTyped = (LockHandle)lockHandle;

            return lockHandleTyped.ReleaseAsync(cancellationToken);
        }

        public Task<bool> RenewAsync(IDistributedLock lockHandle, CancellationToken cancellationToken)
        {
            if (lockHandle is null)
                throw new ArgumentNullException(nameof(lockHandle));

            var lockHandleTyped = (LockHandle)lockHandle;

            return lockHandleTyped.RenewAsync(cancellationToken);
        }

        public async Task<IDistributedLock> TryLockAsync(string account, string lockId, string lockOwnerId, string proposedLeaseId, TimeSpan leasePeriod, CancellationToken cancellationToken)
        {
            var lockBlob = await GetLockBlobAsync(account, lockId)
                .ConfigureAwait(false);

            var leaseId = await TryAcquireLeaseAsync(lockBlob, leasePeriod, proposedLeaseId, cancellationToken)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(leaseId))
                return null;

            if (!string.IsNullOrEmpty(lockOwnerId))
            {
                lockBlob.Metadata.Add(OWNERID_METADATA, lockOwnerId);

                await lockBlob
                    .SetMetadataAsync(accessCondition: new AccessCondition { LeaseId = leaseId }, options: null, operationContext: null, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }

            return new LockHandle(account, lockId, lockBlob, leaseId, leasePeriod);
        }

        private static async Task<string> TryAcquireLeaseAsync(CloudBlockBlob lockBlob, TimeSpan lockPeriod, string proposedLeaseId, CancellationToken cancellationToken)
        {
            try
            {
                return await lockBlob
                    .AcquireLeaseAsync(lockPeriod, proposedLeaseId, accessCondition: null, options: null, operationContext: null, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (StorageException exc) when ((exc.RequestInformation?.HttpStatusCode).GetValueOrDefault() == 409)
            {
                return null;
            }
            catch (StorageException exc) when ((exc.RequestInformation?.HttpStatusCode).GetValueOrDefault() == 404)
            {
                // swallow and resume - file to acquire lease for doesn't exist
            }

            try
            {
                await lockBlob.UploadTextAsync(string.Empty).ConfigureAwait(false);
            }
            catch
            {
                // swallow - there is a chance the file was create in the meantime
            }

            try
            {
                return await lockBlob
                    .AcquireLeaseAsync(lockPeriod, proposedLeaseId, accessCondition: null, options: null, operationContext: null, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (StorageException exc) when ((exc.RequestInformation?.HttpStatusCode).GetValueOrDefault() == 409)
            {
                return null;
            }
        }

        internal class LockHandle : IDistributedLock
        {
            private readonly CloudBlockBlob lockBlob;
            private readonly string leaseId;
            private readonly TimeSpan leasePeriod;

            public LockHandle(string account, string lockId, CloudBlockBlob lockBlob, string leaseId, TimeSpan leasePeriod)
            {
                Account = account;
                LockId = lockId;

                this.lockBlob = lockBlob;
                this.leaseId = leaseId;
                this.leasePeriod = leasePeriod;
            }

            public string LockId { get; }

            public string Account { get; }

            public async Task<bool> RenewAsync(CancellationToken cancellationToken)
            {
                try
                {
                    await lockBlob
                        .RenewLeaseAsync(new AccessCondition { LeaseId = leaseId }, null, null, cancellationToken)
                        .ConfigureAwait(false);

                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public async Task<bool> ReleaseAsync(CancellationToken cancellationToken)
            {
                try
                {
                    await lockBlob
                        .ReleaseLeaseAsync(new AccessCondition { LeaseId = leaseId }, null, null, cancellationToken)
                        .ConfigureAwait(false);

                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
