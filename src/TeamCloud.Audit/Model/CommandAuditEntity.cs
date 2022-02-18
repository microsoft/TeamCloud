/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Runtime.Serialization;
using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Audit.Model;

[JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
public sealed class CommandAuditEntity : ITableEntity
{
    internal const string AUDIT_TABLE_NAME = "AuditCommands";
    internal const string AUDIT_CONTAINER_NAME = "audit-commands";

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public CommandAuditEntity()
    { }

    public CommandAuditEntity(ICommand command) : this()
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        RowKey = command.CommandId.ToString();

        PartitionKey = Guid.TryParse(command.OrganizationId, out var organizationId)
            ? organizationId.ToString()
            : Guid.Empty.ToString();

        ProjectId = Guid.TryParse(command.ProjectId, out var projectId)
            ? projectId.ToString()
            : Guid.Empty.ToString();

        ComponentTask = (command as ComponentTaskRunCommand)?.Payload?.TypeName
            ?? (command as ComponentTaskRunCommand)?.Payload?.Type.ToString()
            ?? string.Empty;

        UserId = command.User.Id.ToString();
        ParentId = command.ParentId.ToString();
        Command = command.GetTypeName(prettyPrint: true);
    }

    public string CommandId => RowKey;

    public string OrganizationId => PartitionKey;

    public string CommandJson { get; set; }

    public string ResultJson { get; set; }

    public string ProjectId { get; set; }
    public string UserId { get; set; }
    public string ParentId { get; set; }

    public string Command { get; set; }
    public string ComponentTask { get; set; }

    public CommandRuntimeStatus RuntimeStatus { get; set; } = CommandRuntimeStatus.Unknown;
    public string CustomStatus { get; set; }
    public string Errors { get; set; }

    public DateTime? Created { get; set; }
    public DateTime? Updated { get; set; }
}
