/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;

namespace TeamCloud.Adapters;

#pragma warning disable CS0618 // Type or member is obsolete

public sealed class AdapterLock : IDistributedLock, IAsyncDisposable
{

    private readonly IDistributedLockManager lockManager;
    private readonly IDistributedLock lockHandle;

    internal AdapterLock(IDistributedLockManager lockManager, IDistributedLock lockHandle)
    {
        this.lockManager = lockManager ?? throw new ArgumentNullException(nameof(lockManager));
        this.lockHandle = lockHandle ?? throw new ArgumentNullException(nameof(lockHandle));
    }

    public string LockId => lockHandle.LockId;



    public async ValueTask DisposeAsync()
    {
        await lockManager
            .ReleaseLockAsync(lockHandle, CancellationToken.None)
            .ConfigureAwait(false);
    }
}
