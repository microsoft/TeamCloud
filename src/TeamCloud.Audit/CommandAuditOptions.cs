/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Audit
{
    public sealed class CommandAuditOptions : ICommandAuditOptions
    {
        public static ICommandAuditOptions Default
            => new CommandAuditOptions();

        private CommandAuditOptions()
        { }

        public string ConnectionString { get; set; }
            = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        public string StoragePrefix { get; set; }
    }
}
