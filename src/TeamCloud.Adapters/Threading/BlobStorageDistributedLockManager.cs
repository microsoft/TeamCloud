/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.WebJobs.Host;
using TeamCloud.Azure.Storage;

namespace TeamCloud.Adapters.Threading;

#pragma warning disable CS0618 // Type or member is obsolete

// IDistributedLockManager is marked as obsolete, because it's not ready for "prime time"
// however; it is used to managed singleton function execution within the functions fx !!!

public sealed class BlobStorageDistributedLockManager : IDistributedLockManager
{

#pragma warning restore CS0608

    private const string OWNERID_METADATA = "OwnerId";
    private const string CONTAINER_NAME = "distributed-locks";

    private readonly IBlobService blobs;
    private readonly IBlobStorageDistributedLockOptions options;

    public BlobStorageDistributedLockManager(IBlobService blobs, IBlobStorageDistributedLockOptions options = null)
    {
        this.blobs = blobs ?? throw new ArgumentNullException(nameof(blobs));
        this.options = options ?? BlobStorageDistributedLockOptions.Default;
    }

    public Task<bool> RenewAsync(IDistributedLock lockHandle, CancellationToken cancellationToken)
    {
        LockHandle lockHandleTyped = (LockHandle)lockHandle;
        return lockHandleTyped.RenewAsync(cancellationToken);
    }

    public async Task ReleaseLockAsync(IDistributedLock lockHandle, CancellationToken cancellationToken)
    {
        LockHandle lockHandleTyped = (LockHandle)lockHandle;
        await ReleaseLeaseAsync(lockHandleTyped.BlobLeaseClient, lockHandleTyped.LeaseId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<string> GetLockOwnerAsync(string account, string lockId, CancellationToken cancellationToken)
    {
        var containerClient = await GetContainerClientAsync(cancellationToken)
            .ConfigureAwait(false);

        var lockBlob = containerClient
            .GetBlobClient(GetLockPath(lockId));

        var blobProperties = await ReadLeaseBlobMetadataAsync(lockBlob, cancellationToken)
            .ConfigureAwait(false);

        // if the lease is Available, then there is no current owner
        // (any existing owner value is the last owner that held the lease)
        if (blobProperties != null &&
            blobProperties.LeaseState == LeaseState.Available &&
            blobProperties.LeaseStatus == LeaseStatus.Unlocked)
        {
            return null;
        }

        string owner = default;
        blobProperties?.Metadata.TryGetValue(OWNERID_METADATA, out owner);
        return owner;
    }

    public async Task<IDistributedLock> TryLockAsync(string account, string lockId, string lockOwnerId, string proposedLeaseId, TimeSpan lockPeriod, CancellationToken cancellationToken)
    {
        var containerClient = await GetContainerClientAsync(cancellationToken)
            .ConfigureAwait(false);

        var lockBlob = containerClient
            .GetBlobClient(GetLockPath(lockId));

        string leaseId = await TryAcquireLeaseAsync(lockBlob, lockPeriod, proposedLeaseId, cancellationToken)
            .ConfigureAwait(false);

        if (string.IsNullOrEmpty(leaseId))
            return null;

        if (!string.IsNullOrEmpty(lockOwnerId))
            await WriteLeaseBlobMetadataAsync(lockBlob, leaseId, lockOwnerId, cancellationToken)
                .ConfigureAwait(false);

        var lockHandle = new LockHandle(leaseId, lockId, lockBlob.GetBlobLeaseClient(leaseId), lockPeriod);

        return lockHandle;
    }

    private Task<BlobContainerClient> GetContainerClientAsync(CancellationToken cancellationToken)
        => blobs.GetBlobContainerClientAsync(options.ConnectionString, CONTAINER_NAME, cancellationToken: cancellationToken);

    private static string GetLockPath(string lockId) => $"locks/{lockId}";

    private static async Task<string> TryAcquireLeaseAsync(BlobClient blobClient, TimeSpan leasePeriod, string proposedLeaseId, CancellationToken cancellationToken)
    {
        bool blobDoesNotExist;

        try
        {
            // Check if a lease is available before trying to acquire. The blob may not
            // yet exist; if it doesn't we handle the 404, create it, and retry below.
            // The reason we're checking to see if the lease is available before trying
            // to acquire is to avoid the flood of 409 errors that Application Insights
            // picks up when a lease cannot be acquired due to conflict; see issue #2318.
            var blobProperties = await ReadLeaseBlobMetadataAsync(blobClient, cancellationToken)
                .ConfigureAwait(false);

            switch (blobProperties?.LeaseState)
            {
                case null:
                case LeaseState.Available:
                case LeaseState.Expired:
                case LeaseState.Broken:
                    var leaseResponse = await blobClient.GetBlobLeaseClient(proposedLeaseId)
                        .AcquireAsync(leasePeriod, cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                    return leaseResponse.Value.LeaseId;
                default:
                    return null;
            }
        }
        catch (RequestFailedException exception)
        {
            if (exception.Status == 409)
            {
                return null;
            }
            else if (exception.Status == 404)
            {
                blobDoesNotExist = true;
            }
            else
            {
                throw;
            }
        }

        if (blobDoesNotExist)
        {
            await TryCreateAsync(blobClient, cancellationToken)
                .ConfigureAwait(false);

            try
            {
                var leaseResponse = await blobClient.GetBlobLeaseClient(proposedLeaseId)
                    .AcquireAsync(leasePeriod, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                return leaseResponse.Value.LeaseId;
            }
            catch (RequestFailedException exception)
            {
                if (exception.Status == 409)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        return null;
    }

    private static async Task ReleaseLeaseAsync(BlobLeaseClient blobLeaseClient, string leaseId, CancellationToken cancellationToken)
    {
        try
        {
            // Note that this call returns without throwing if the lease is expired. See the table at:
            // http://msdn.microsoft.com/en-us/library/azure/ee691972.aspx
            await blobLeaseClient
                .ReleaseAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (RequestFailedException exception)
        {
            if (exception.Status == 404 || exception.Status == 409)
            {
                // if the blob no longer exists, or there is another lease
                // now active, there is nothing for us to release so we can
                // ignore
            }
            else
            {
                throw;
            }
        }
    }

    private static async Task<bool> TryCreateAsync(BlobClient blobClient, CancellationToken cancellationToken)
    {
        bool isContainerNotFoundException;

        try
        {
            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(string.Empty)))
            {
                await blobClient.UploadAsync(stream, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            return true;
        }
        catch (RequestFailedException exception)
        {
            if (exception.Status == 404)
            {
                isContainerNotFoundException = true;
            }
            else if (exception.Status == 409 || exception.Status == 412)
            {
                // The blob already exists, or is leased by someone else
                return false;
            }
            else
            {
                throw;
            }
        }

        Debug.Assert(isContainerNotFoundException);

        var container = blobClient.GetParentBlobContainerClient();

        try
        {
            await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (RequestFailedException exception)
        when (exception.Status == 409 && string.Compare("ContainerBeingDeleted", exception.ErrorCode) == 0)
        {
            throw new RequestFailedException("The host container is pending deletion and currently inaccessible.");
        }

        try
        {
            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(string.Empty)))
            {
                await blobClient.UploadAsync(stream, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }

            return true;
        }
        catch (RequestFailedException exception)
        {
            if (exception.Status == 409 || exception.Status == 412)
            {
                // The blob already exists, or is leased by someone else
                return false;
            }
            else
            {
                throw;
            }
        }
    }

    private static async Task WriteLeaseBlobMetadataAsync(BlobClient blobClient, string leaseId, string lockOwnerId, CancellationToken cancellationToken)
    {
        var blobProperties = await ReadLeaseBlobMetadataAsync(blobClient, cancellationToken)
            .ConfigureAwait(false);

        if (blobProperties != null)
        {
            blobProperties.Metadata[OWNERID_METADATA] = lockOwnerId;
            await blobClient.SetMetadataAsync(blobProperties.Metadata, new BlobRequestConditions
            {
                LeaseId = leaseId
            }, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private static async Task<BlobProperties> ReadLeaseBlobMetadataAsync(BlobClient blobClient, CancellationToken cancellationToken)
    {
        try
        {
            var propertiesResponse = await blobClient
                .GetPropertiesAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return propertiesResponse.Value;
        }
        catch (RequestFailedException exception)
        {
            if (exception.Status == 404)
            {
                // the blob no longer exists
                return null;
            }
            else
            {
                throw;
            }
        }
    }

    internal class LockHandle : IDistributedLock
    {
        private readonly TimeSpan leasePeriod;


        public LockHandle()
        {
        }

        public LockHandle(string leaseId, string lockId, BlobLeaseClient blobLeaseClient, TimeSpan leasePeriod)
        {
            this.LeaseId = leaseId;
            this.LockId = lockId;
            this.leasePeriod = leasePeriod;
            this.BlobLeaseClient = blobLeaseClient;
        }

        public string LeaseId { get; internal set; }

        public string LockId { get; internal set; }

        public BlobLeaseClient BlobLeaseClient { get; internal set; }

        public async Task<bool> RenewAsync(CancellationToken cancellationToken)
        {
            try
            {
                await BlobLeaseClient
                    .RenewAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                // The next execution should occur after a normal delay.
                return true;
            }
            catch (RequestFailedException exception)
            {
                // indicates server-side error
                if (exception.Status >= 500 && exception.Status < 600)
                {
                    return false; // The next execution should occur more quickly (try to renew the lease before it expires).
                }
                else
                {
                    // If we've lost the lease or cannot re-establish it, we want to fail any
                    // in progress function execution
                    throw;
                }
            }
        }
    }
}
