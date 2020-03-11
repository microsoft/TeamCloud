/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities.Activities
{
    public class CommandAuditEntity : TableEntity
    {
        [IgnoreProperty]
        public string InstanceId
        {
            get => this.RowKey;
            set => this.RowKey = value;
        }

        [IgnoreProperty]
        public string CommandId
        {
            get => this.PartitionKey;
            set => this.PartitionKey = value;
        }
        public string Provider { get; set; }
        public string Command { get; set; }
        public string RuntimeStatus { get; set; }
        public string CustomStatus { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Processed { get; set; }
    }
}
