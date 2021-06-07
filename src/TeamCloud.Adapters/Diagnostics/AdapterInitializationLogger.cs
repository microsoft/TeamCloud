/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace TeamCloud.Adapters.Diagnostics
{
    internal sealed class AdapterInitializationLogger : ILogger, IDisposable
    {
        private readonly ILogger logger;
        private readonly StreamWriter writer;

        private bool disposed;

        public AdapterInitializationLogger(ILogger logger, Stream stream)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.writer = new StreamWriter(stream ?? throw new ArgumentNullException(nameof(stream)));
        }

        ~AdapterInitializationLogger()
        {
            Dispose(disposing: false);
        }

        public IDisposable BeginScope<TState>(TState state)
            => logger.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel)
            => true; // capture all log levels

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel >= LogLevel.Information)
            {
                try
                {
                    writer.WriteLine($"{logLevel,-12} - {formatter(state, exception)}");
                    writer.Flush(); // flush to update component task output file
                }
                catch
                {
                    // swallow and move on
                }
            }

            if (logger.IsEnabled(logLevel))
            {
                logger.Log(logLevel, eventId, state, exception, formatter);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    writer.Close();
                }

                disposed = true;
            }
        }


        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
