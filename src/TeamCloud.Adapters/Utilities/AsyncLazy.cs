/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace TeamCloud.Adapters.Utilities
{
    internal sealed class AsyncLazy<T> : ResetLazy<Task<T>>
    {
        public AsyncLazy(Func<T> valueFactory, LazyThreadSafetyMode lazyThreadSafetyMode = LazyThreadSafetyMode.None)
            : base(() => Task.Factory.StartNew(valueFactory, default, TaskCreationOptions.AttachedToParent, TaskScheduler.Current), lazyThreadSafetyMode)
        { }

        public AsyncLazy(Func<Task<T>> taskFactory, LazyThreadSafetyMode lazyThreadSafetyMode = LazyThreadSafetyMode.None)
            : base(() => taskFactory(), lazyThreadSafetyMode)
        { }

        public TaskAwaiter<T> GetAwaiter()
            => Value.GetAwaiter();
    }
}
