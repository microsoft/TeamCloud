/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;

namespace TeamCloud.Orchestration.Threading
{
    public static class DistributedLockManagerExtensions
    {
        public static async Task<IDistributedLock> AcquireLockAsync(this IDistributedLockManager distributedLockManager, string lockId, string lockOwner, TimeSpan? lockPeriod = default, TimeSpan? acquisitionTimeout = default, CancellationToken cancellationToken = default)
        {
            if (distributedLockManager is null)
                throw new ArgumentNullException(nameof(distributedLockManager));

            if (string.IsNullOrEmpty(lockId))
                throw new ArgumentException($"'{nameof(lockId)}' cannot be null or empty", nameof(lockId));

            if (string.IsNullOrEmpty(lockOwner))
                throw new ArgumentException($"'{nameof(lockOwner)}' cannot be null or empty", nameof(lockOwner));

            using var acquisitionCancellationTokenSource = new CancellationTokenSource(acquisitionTimeout.GetValueOrDefault(TimeSpan.FromMinutes(1)));
            using var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(acquisitionCancellationTokenSource.Token, cancellationToken);

            while (!linkedCancellationTokenSource.Token.IsCancellationRequested)
            {
                var distributedLock = await distributedLockManager
                    .TryLockAsync(null, lockId, lockOwner, null, lockPeriod.GetValueOrDefault(TimeSpan.FromMinutes(1)), linkedCancellationTokenSource.Token)
                    .ConfigureAwait(false);

                if (distributedLock != null)
                    return distributedLock;
            }

            throw new TimeoutException($"Unable to acquire lock {lockId} for owner {lockOwner}");
        }
    }
}
