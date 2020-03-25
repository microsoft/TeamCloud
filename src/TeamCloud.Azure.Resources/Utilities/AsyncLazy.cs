/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TeamCloud.Azure.Resources.Utilities
{
    internal sealed class AsyncLazy<T> : ResetLazy<Task<T>>
    {
        public AsyncLazy(Func<T> valueFactory) : base(() => Task.Factory.StartNew(valueFactory, default, TaskCreationOptions.None, TaskScheduler.Current))
        { }

        public AsyncLazy(Func<Task<T>> taskFactory) : base(() => Task.Factory.StartNew(() => taskFactory(), default, TaskCreationOptions.None, TaskScheduler.Current).Unwrap())
        { }

        public TaskAwaiter<T> GetAwaiter()
            => Value.GetAwaiter();

        public ConfiguredTaskAwaitable<T> ConfigureAwait(bool continueOnCapturedContext)
            => Value.ConfigureAwait(continueOnCapturedContext);
    }
}
