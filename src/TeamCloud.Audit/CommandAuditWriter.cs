/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Blob;
using TeamCloud.Audit.Model;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Serialization;
using BlobCloudStorageAccount = Microsoft.Azure.Storage.CloudStorageAccount;
using TableCloudStorageAccount = Microsoft.Azure.Cosmos.Table.CloudStorageAccount;

namespace TeamCloud.Audit
{

    public sealed class CommandAuditWriter : ICommandAuditWriter
    {
        private static readonly DateTime MinDateTime = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

        private readonly ICommandAuditOptions options;
        private readonly Lazy<CloudBlobContainer> auditContainerInstance;
        private readonly Lazy<CloudTable> auditTableInstance;

        public CommandAuditWriter(ICommandAuditOptions options = null)
        {
            this.options = options ?? CommandAuditOptions.Default;

            auditContainerInstance = new Lazy<CloudBlobContainer>(() => BlobCloudStorageAccount
                .Parse(this.options.ConnectionString)
                .CreateCloudBlobClient().GetContainerReference(CommandAuditEntity.AUDIT_CONTAINER_NAME));

            auditTableInstance = new Lazy<CloudTable>(() => TableCloudStorageAccount
                .Parse(this.options.ConnectionString)
                .CreateCloudTableClient().GetTableReference(CommandAuditEntity.AUDIT_TABLE_NAME));
        }

        public Task WriteAsync(ICommand command, ICommandResult commandResult = default) => Task.WhenAll
        (
            WriteContainerAsync(command, commandResult),
            WriteTableAsync(command, commandResult)
        );

        private async Task WriteContainerAsync(ICommand command, ICommandResult commandResult)
        {
            var auditContainer = await auditContainerInstance
                .EnsureContainerAsync()
                .ConfigureAwait(false);

            if (command.CommandId.Equals(commandResult?.CommandId))
            {
                commandResult.OrganizationId = command.OrganizationId;
            }

            Task.WaitAll
            (
                WriteBlobAsync(command, command?.GetPath()),
                WriteBlobAsync(commandResult, commandResult?.GetPath())
            );

            Task WriteBlobAsync(object auditData, string auditPath)
            {
                if (auditData is null || string.IsNullOrEmpty(auditPath))
                    return Task.CompletedTask; // no data object or path was given

                var auditBlob = auditContainer.GetBlockBlobReference(auditPath);

                return auditBlob
                    .UploadTextAsync(TeamCloudSerialize.SerializeObject(auditData));
            }
        }

        private async Task WriteTableAsync(ICommand command, ICommandResult commandResult)
        {
            var auditEntity = new CommandAuditEntity(command);

            var auditTable = await auditTableInstance
                .EnsureTableAsync()
                .ConfigureAwait(false);

            var entityResult = await auditTable
                .ExecuteAsync(TableOperation.Retrieve<CommandAuditEntity>(auditEntity.Entity.PartitionKey, auditEntity.Entity.RowKey))
                .ConfigureAwait(false);

            if (entityResult.HttpStatusCode == (int)HttpStatusCode.OK)
                auditEntity = entityResult.Result as CommandAuditEntity ?? auditEntity;

            var timestamp = DateTime.UtcNow;

            auditEntity.Created = GetTableStorageMinDate(auditEntity.Created, commandResult?.CreatedTime, timestamp);
            auditEntity.Updated = GetTableStorageMaxDate(auditEntity.Updated, commandResult?.LastUpdatedTime, timestamp);

            if (commandResult != null)
            {
                auditEntity.RuntimeStatus = commandResult.RuntimeStatus;
                auditEntity.CustomStatus = commandResult.CustomStatus ?? string.Empty;
                auditEntity.Errors = string.Join(Environment.NewLine, commandResult.Errors.Select(error => $"[{error.Severity}] {error.Message}"));
            }

            await auditTable
                .ExecuteAsync(TableOperation.InsertOrReplace(auditEntity))
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
}
