/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Microsoft.WindowsAzure.Storage.Table;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Model.Auditing
{
    public sealed class CommandAuditEntity : TableEntityBase
    {
        [IgnoreProperty]
        public string InstanceId
        {
            get => this.TableEntity.RowKey;
            set => this.TableEntity.RowKey = value;
        }

        [IgnoreProperty]
        public string CommandId
        {
            get => this.TableEntity.PartitionKey;
            set => this.TableEntity.PartitionKey = value;
        }

        public string Provider { get; set; }
        public string Command { get; set; }
        public string Project { get; set; }
        public CommandRuntimeStatus Status { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Sent { get; set; }
        public DateTime? Processed { get; set; }
        public DateTime? Timeout { get; set; }
        public string[] Errors { get; set; }
    }
}
