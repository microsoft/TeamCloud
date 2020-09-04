/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.WindowsAzure.Storage.Table;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestration.Auditing.Model
{
    public sealed class CommandAuditEntity : TableEntityBase
    {
        private static readonly string NoneProjectPartitionKey = Guid.Empty.ToString();

        public CommandAuditEntity()
        { }

        public CommandAuditEntity(ICommand command, string providerId = null) : this()
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            TableEntity.PartitionKey = command.ProjectId ?? NoneProjectPartitionKey;
            TableEntity.RowKey = $"{command.CommandId}@{providerId}".TrimEnd('@');

            CommandId = command.CommandId.ToString();
            Command = command.GetType().Name;
            Event = (command as ProviderEventCommand)?.Payload?.EventType ?? string.Empty;
            Provider = providerId ?? string.Empty;
        }

        [IgnoreProperty]
        public string ProjectId => this.TableEntity.PartitionKey;

        [IgnoreProperty]
        public string AuditId => this.TableEntity.RowKey;

        [Column(Order = 101)]
        public string CommandId { get; private set; }
        [Column(Order = 102)]
        public string Command { get; private set; }
        [Column(Order = 103)]
        public string Event { get; private set; }
        [Column(Order = 104)]
        public string Provider { get; private set; }

        [Column(Order = 201)]
        public CommandRuntimeStatus RuntimeStatus { get; set; } = CommandRuntimeStatus.Unknown;
        [Column(Order = 202)]
        public string CustomStatus { get; set; }
        [Column(Order = 203)]
        public string Errors { get; set; }

        [Column(Order = 301)]
        public DateTime? Created { get; set; }
        [Column(Order = 302)]
        public DateTime? Updated { get; set; }
        [Column(Order = 303)]
        public DateTime? Sent { get; set; }
        [Column(Order = 304)]
        public DateTime? Processed { get; set; }
        [Column(Order = 305)]
        public DateTime? Timeout { get; set; }
    }
}
