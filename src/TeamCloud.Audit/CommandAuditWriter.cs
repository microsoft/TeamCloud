/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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

        private static string SanitizeStoragePrefix(ICommandAuditOptions options)
        {
            const int MaxStorageNameSize = 45;

            var validStorageNameCharacters = (options.StoragePrefix ?? string.Empty)
                    .ToCharArray()
                    .Where(char.IsLetterOrDigit);

            if (!validStorageNameCharacters.Any())
                return string.Empty;

            if (char.IsNumber(validStorageNameCharacters.First()))
            {
                // Azure Table storage requires that the task hub does not start
                // with a number. If it does, prepend "a" to the beginning.

                validStorageNameCharacters = validStorageNameCharacters.ToList();
                ((List<char>)validStorageNameCharacters).Insert(0, 'a');
            }

            return new string(validStorageNameCharacters
                .Take(MaxStorageNameSize)
                .ToArray());
        }

        private static string GetAuditContainerName(ICommandAuditOptions options)
            => $"{SanitizeStoragePrefix(options)}-Audit".Trim().TrimStart('-').ToLowerInvariant();

        private static string GetAuditTableName(ICommandAuditOptions options)
            => $"{SanitizeStoragePrefix(options)}Audit".Trim();

        private readonly Lazy<CloudBlobContainer> auditContainer;
        private readonly Lazy<CloudTable> auditTable;

        public CommandAuditWriter(ICommandAuditOptions options = null)
        {
            auditContainer = new Lazy<CloudBlobContainer>(() => BlobCloudStorageAccount
                .Parse((options ?? CommandAuditOptions.Default).ConnectionString)
                .CreateCloudBlobClient().GetContainerReference(GetAuditContainerName(options)));

            auditTable = new Lazy<CloudTable>(() => TableCloudStorageAccount
                .Parse((options ?? CommandAuditOptions.Default).ConnectionString)
                .CreateCloudTableClient().GetTableReference(GetAuditTableName(options)));
        }

        public Task AuditAsync(ICommand command, ICommandResult commandResult = default) => Task.WhenAll
        (
            WriteContainerAsync(command, commandResult),
            WriteTableAsync(command, commandResult)
        );

        private async Task<CloudBlobContainer> GetAuditContainerAsync()
        {
            if (!auditContainer.IsValueCreated)
            {
                _ = await auditContainer.Value
                    .CreateIfNotExistsAsync()
                    .ConfigureAwait(false);
            }

            return auditContainer.Value;
        }

        private Task WriteContainerAsync(ICommand command, ICommandResult commandResult)
        {
            return Task.WhenAll
            (
                WriteBlobAsync(command),
                commandResult is null ? Task.CompletedTask : WriteBlobAsync(commandResult)
            );

            async Task WriteBlobAsync(object data)
            {
                var auditPath = $"{command.OrganizationId}/{command.ProjectId}/{command.CommandId}/{data.GetType().Name}.json";

                var auditContainer = await GetAuditContainerAsync().ConfigureAwait(false);
                var auditBlob = auditContainer.GetBlockBlobReference(auditPath.Replace("//", "/", StringComparison.OrdinalIgnoreCase));

                await auditBlob
                    .UploadTextAsync(TeamCloudSerialize.SerializeObject(data))
                    .ConfigureAwait(false);
            }
        }

        private async Task<CloudTable> GetAuditTableAsync()
        {
            if (!auditTable.IsValueCreated)
            {
                _ = await auditTable.Value
                    .CreateIfNotExistsAsync()
                    .ConfigureAwait(false);
            }

            return auditTable.Value;
        }

        private async Task WriteTableAsync(ICommand command, ICommandResult commandResult)
        {
            var entity = new CommandAuditEntity(command);

            var auditTable = await GetAuditTableAsync()
                .ConfigureAwait(false);

            var entityResult = await auditTable
                .ExecuteAsync(TableOperation.Retrieve<CommandAuditEntity>(entity.PartitionKey, entity.RowKey))
                .ConfigureAwait(false);

            if (entityResult.HttpStatusCode == (int)HttpStatusCode.OK)
                entity = entityResult.Result as CommandAuditEntity ?? entity;

            var timestamp = DateTime.UtcNow;

            entity.Created = GetTableStorageMinDate(entity.Created, commandResult?.CreatedTime, timestamp);
            entity.Updated = GetTableStorageMaxDate(entity.Updated, commandResult?.LastUpdatedTime, timestamp);

            if (commandResult != null)
            {
                entity.RuntimeStatus = commandResult.RuntimeStatus;
                entity.CustomStatus = commandResult.CustomStatus ?? string.Empty;
                entity.Errors = string.Join(Environment.NewLine, commandResult.Errors.Select(error => $"[{error.Severity}] {error.Message}"));
            }

            await auditTable
                .ExecuteAsync(TableOperation.InsertOrReplace(entity))
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
