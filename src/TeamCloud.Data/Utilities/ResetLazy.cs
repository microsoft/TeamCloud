/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading;

namespace TeamCloud.Data.Utilities
{
    internal class ResetLazy<T>
    {
        private readonly Func<T> factory;

        private Lazy<T> lazy = null;

        public ResetLazy(Func<T> valueFactory, bool isThreadSafe)
            : this(valueFactory, isThreadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None)
        { }

        public ResetLazy(Func<T> valueFactory, LazyThreadSafetyMode lazyThreadSafetyMode = LazyThreadSafetyMode.None)
        {
            factory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
            LazyThreadSafetyMode = lazyThreadSafetyMode;
            Reset();
        }


        public T Value
            => lazy.Value;

        public bool IsValueCreated
            => lazy.IsValueCreated;

        public LazyThreadSafetyMode LazyThreadSafetyMode { get; }

        public void Reset()
            => Interlocked.Exchange(ref lazy, new Lazy<T>(factory, LazyThreadSafetyMode));
    }
}
