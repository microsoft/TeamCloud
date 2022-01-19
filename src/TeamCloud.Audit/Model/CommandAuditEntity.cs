/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Audit.Model;

[JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
public sealed class CommandAuditEntity : AuditEntity
{
    internal const string AUDIT_TABLE_NAME = "AuditCommands";
    internal const string AUDIT_CONTAINER_NAME = "audit-commands";

    public CommandAuditEntity()
    { }

    public CommandAuditEntity(ICommand command) : this()
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        Entity.RowKey = command.CommandId.ToString();

        Entity.PartitionKey = Guid.TryParse(command.OrganizationId, out var organizationId)
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

    [IgnoreProperty]
    public string CommandId => Entity.RowKey;

    [IgnoreProperty]
    public string OrganizationId => Entity.PartitionKey;

    [IgnoreProperty]
    public string CommandJson { get; set; }

    [IgnoreProperty]
    public string ResultJson { get; set; }

    public string ProjectId { get; private set; }
    public string UserId { get; private set; }
    public string ParentId { get; private set; }

    public string Command { get; private set; }
    public string ComponentTask { get; private set; }

    public CommandRuntimeStatus RuntimeStatus { get; set; } = CommandRuntimeStatus.Unknown;
    public string CustomStatus { get; set; }
    public string Errors { get; set; }

    public DateTime? Created { get; set; }
    public DateTime? Updated { get; set; }
}
