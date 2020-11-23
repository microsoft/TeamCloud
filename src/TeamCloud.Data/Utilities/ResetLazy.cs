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

        private Lazy<T> lazy;

        public ResetLazy(Func<T> valueFactory, bool isThreadSafe)
            : this(valueFactory, isThreadSafe ? LazyThreadSafetyMode.PublicationOnly : LazyThreadSafetyMode.None)
        { }

        public ResetLazy(Func<T> valueFactory, LazyThreadSafetyMode lazyThreadSafetyMode = LazyThreadSafetyMode.PublicationOnly)
        {
            factory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
            lazy = new Lazy<T>(factory, LazyThreadSafetyMode);
            LazyThreadSafetyMode = lazyThreadSafetyMode;
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
