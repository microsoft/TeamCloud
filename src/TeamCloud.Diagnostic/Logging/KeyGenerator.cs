using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Serilog.Events;
using Serilog.Sinks.AzureTableStorage.KeyGenerator;

namespace TeamCloud.Diagnostic.Logging
{
    internal sealed class KeyGenerator : DefaultKeyGenerator
    {
        private static readonly string InstanceId = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") ?? Environment.MachineName;

        public override string GenerateRowKey(LogEvent logEvent, string suffix = null)
        {
            var rowId = Interlocked.Increment(ref base.RowId);

            return $"{InstanceId}|{Process.GetCurrentProcess().Id:D10}|{Unsafe.As<long, ulong>(ref rowId):D20}";
        }
    }
}
