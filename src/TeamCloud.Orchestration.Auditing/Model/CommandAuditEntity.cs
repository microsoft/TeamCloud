/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Microsoft.WindowsAzure.Storage.Table;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestration.Auditing.Model
{
    public sealed class CommandAuditEntity : TableEntityBase
    {
        public CommandAuditEntity()
        { }

        public CommandAuditEntity(ICommand command, Provider provider = default) : this()
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            TableEntity.PartitionKey = command.ProjectId;
            TableEntity.RowKey = $"{command.CommandId}@{provider?.Id}".TrimEnd('@');
        }

        [IgnoreProperty]
        public string ProjectId => this.TableEntity.PartitionKey;

        [IgnoreProperty]
        public string AuditId => this.TableEntity.RowKey;

        public string CommandId { get; set; }
        public string Command { get; set; }
        public string Provider { get; set; }
        public CommandRuntimeStatus Status { get; set; } = CommandRuntimeStatus.Unknown;

        public DateTime? Created { get; set; }
        public DateTime? Sent { get; set; }
        public DateTime? Processed { get; set; }
        public DateTime? Timeout { get; set; }
    }
}
