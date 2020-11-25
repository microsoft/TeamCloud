/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TeamCloud.Data.Utilities
{
    internal sealed class AsyncLazy<T> : ResetLazy<Task<T>>
    {
        public AsyncLazy(Func<T> valueFactory) : base(() => Task.Factory.StartNew(valueFactory, default, TaskCreationOptions.AttachedToParent, TaskScheduler.Current))
        { }

        public AsyncLazy(Func<Task<T>> taskFactory) : base(() => taskFactory())
        { }

        public TaskAwaiter<T> GetAwaiter()
            => Value.GetAwaiter();
    }
}
