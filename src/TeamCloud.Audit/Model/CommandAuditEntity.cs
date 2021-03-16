/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.Cosmos.Table;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Audit.Model
{
    public sealed class CommandAuditEntity : TableEntityBase
    {
        private static readonly string EmptyKey = Guid.Empty.ToString();

        public CommandAuditEntity()
        { }

        public CommandAuditEntity(ICommand command) : this()
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            PartitionKey = $"{command.OrganizationId ?? EmptyKey}|{command.ProjectId ?? EmptyKey}";
            RowKey = command.CommandId.ToString();

            CommandId = command.CommandId.ToString();
            Command = command.GetType().Name;

            ComponentTask = (command as ComponentTaskRunCommand)?.Payload?.TypeName ?? (command as ComponentTaskRunCommand)?.Payload?.Type.ToString() ?? string.Empty;
        }

        [IgnoreProperty]
        public string ProjectId => PartitionKey;

        [IgnoreProperty]
        public string AuditId => RowKey;

        [Column(Order = 101)]
        public string CommandId { get; private set; }
        [Column(Order = 102)]
        public string Command { get; private set; }

        [Column(Order = 201)]
        public string ComponentTask { get; private set; }

        [Column(Order = 301)]
        public CommandRuntimeStatus RuntimeStatus { get; set; } = CommandRuntimeStatus.Unknown;
        [Column(Order = 302)]
        public string CustomStatus { get; set; }
        [Column(Order = 303)]
        public string Errors { get; set; }

        [Column(Order = 901)]
        public DateTime? Created { get; set; }
        [Column(Order = 902)]
        public DateTime? Updated { get; set; }
    }
}
