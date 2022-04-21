/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using TeamCloud.Audit.Model;
using TeamCloud.Azure.Storage;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Audit;

public sealed class CommandAuditWriter : ICommandAuditWriter
{
    private static readonly DateTime MinDateTime = new(1601, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

    private readonly IStorageService storage;
    private readonly ICommandAuditOptions options;

    public CommandAuditWriter(IStorageService storageService, ICommandAuditOptions options = null)
    {
        this.storage = storageService ?? throw new ArgumentNullException(nameof(storageService));
        this.options = options ?? CommandAuditOptions.Default;
    }

    public Task WriteAsync(ICommand command, ICommandResult commandResult = default) => Task.WhenAll
    (
        WriteContainerAsync(command, commandResult),
        WriteTableAsync(command, commandResult)
    );

    private async Task WriteContainerAsync(ICommand command, ICommandResult commandResult)
    {
        var blobContainerClient = await storage.Blobs
            .GetBlobContainerClientAsync(options.ConnectionString, CommandAuditEntity.AUDIT_CONTAINER_NAME)
            .ConfigureAwait(false);

        if (command.CommandId.Equals(commandResult?.CommandId))
        {
            commandResult.OrganizationId = command.OrganizationId;
        }

        await Task.WhenAll(

            WriteBlobAsync(blobContainerClient, command, command?.GetPath()),
            WriteBlobAsync(blobContainerClient, commandResult, commandResult?.GetPath())

        ).ConfigureAwait(false);

        static Task WriteBlobAsync(BlobContainerClient containerClient, object auditData, string auditPath)
        {
            if (auditData is null || string.IsNullOrEmpty(auditPath))
                return Task.CompletedTask; // no data object or path was given

            return containerClient
                .GetBlobClient(auditPath)
                .UploadAsync(BinaryData.FromString(TeamCloudSerialize.SerializeObject(auditData)), overwrite: true);
        }
    }

    private async Task WriteTableAsync(ICommand command, ICommandResult commandResult)
    {
        var auditEntity = new CommandAuditEntity(command);

        var tableClient = await storage.Tables
            .GetTableClientAsync(options.ConnectionString, CommandAuditEntity.AUDIT_TABLE_NAME)
            .ConfigureAwait(false);

        try
        {
            var entityResult = await tableClient
                .GetEntityAsync<CommandAuditEntity>(auditEntity.PartitionKey, auditEntity.RowKey)
                .ConfigureAwait(false);

            if (entityResult.Value is not null)
                auditEntity = entityResult.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // doesn't exist, ignore and keep moving
        }

        var timestamp = DateTime.UtcNow;

        auditEntity.Created = GetTableStorageMinDate(auditEntity.Created, commandResult?.CreatedTime, timestamp);
        auditEntity.Updated = GetTableStorageMaxDate(auditEntity.Updated, commandResult?.LastUpdatedTime, timestamp);

        if (commandResult is not null)
        {
            auditEntity.RuntimeStatus = commandResult.RuntimeStatus;
            auditEntity.CustomStatus = commandResult.CustomStatus ?? string.Empty;
            auditEntity.Errors = string.Join(Environment.NewLine, commandResult.Errors.Select(error => $"[{error.Severity}] {error.Message}"));
        }

        await tableClient
            .UpsertEntityAsync(auditEntity, TableUpdateMode.Replace)
            .ConfigureAwait(false);

        static DateTime? GetTableStorageMaxDate(params DateTime?[] dateTimes)
        {
            DateTime? timestamp = null;

            foreach (var dateTime in dateTimes.Where(dt => dt.HasValue && dt > MinDateTime))
            {
                timestamp = timestamp.HasValue
                    ? timestamp > dateTime ? timestamp : dateTime
                    : dateTime;
            }

            return timestamp;
        }

        static DateTime? GetTableStorageMinDate(params DateTime?[] dateTimes)
        {
            DateTime? timestamp = null;

            foreach (var dateTime in dateTimes.Where(dt => dt.HasValue && dt > MinDateTime))
            {
                timestamp = timestamp.HasValue
                    ? timestamp < dateTime ? timestamp : dateTime
                    : dateTime;
            }

            return timestamp;
        }
    }
}
