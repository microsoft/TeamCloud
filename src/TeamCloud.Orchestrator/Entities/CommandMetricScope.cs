/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Orchestrator.Entities
{
    internal class CommandMetricScope : IDisposable
    {
        private bool disposed;
        private readonly Action disposeCallback;

        public CommandMetricScope(Action disposeCallback)
        {
            this.disposeCallback = disposeCallback ?? throw new ArgumentNullException(nameof(disposeCallback));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;

                if (disposing)
                {
                    disposeCallback();
                }
            }
        }

        public void Dispose() => Dispose(true);
    }
}
