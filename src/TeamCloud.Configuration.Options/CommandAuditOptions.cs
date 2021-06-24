/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Audit;

namespace TeamCloud.Configuration.Options
{
    [Options("Audit")]
    public sealed class CommandAuditOptions : ICommandAuditOptions
    {
        public string ConnectionString { get; set; }

        public string StoragePrefix { get; set; }
    }
}
