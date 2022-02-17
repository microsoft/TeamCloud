/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using TeamCloud.Audit.Model;

namespace TeamCloud.Audit;

public class CommandAuditReader : ICommandAuditReader
{
    private readonly ICommandAuditOptions options;
    private readonly Lazy<TableClient> tableClientInstance;
    private readonly Lazy<BlobContainerClient> blobContainerClientInstance;

    public CommandAuditReader(ICommandAuditOptions options = null)
    {
        this.options = options ?? CommandAuditOptions.Default;

        tableClientInstance = new Lazy<TableClient>(() =>
            new TableClient(this.options.ConnectionString, CommandAuditEntity.AUDIT_TABLE_NAME));

        blobContainerClientInstance = new Lazy<BlobContainerClient>(() =>
            new BlobContainerClient(this.options.ConnectionString, CommandAuditEntity.AUDIT_CONTAINER_NAME));
    }

    public async Task<CommandAuditEntity> GetAsync(Guid organizationId, Guid commandId, bool includeJsonDumps = false)
    {
        var tableClient = await tableClientInstance
            .EnsureTableAsync()
            .ConfigureAwait(false);

        CommandAuditEntity entity = null;

        try
        {
            var response = await tableClient
                .GetEntityAsync<CommandAuditEntity>(organizationId.ToString(), commandId.ToString())
                .ConfigureAwait(false);

            entity = response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // doesn't exist
            return null;
        }

        if (entity is not null && includeJsonDumps)
        {
            var blobContainerClient = await blobContainerClientInstance
                .EnsureContainerAsync()
                .ConfigureAwait(false);

            await Task.WhenAll(

                ReadBlobAsync(blobContainerClient, entity.GetCommandPath())
                    .ContinueWith(t => entity.CommandJson = t.Result, TaskContinuationOptions.OnlyOnRanToCompletion),

                ReadBlobAsync(blobContainerClient, entity.GetResultPath())
                    .ContinueWith(t => entity.ResultJson = t.Result, TaskContinuationOptions.OnlyOnRanToCompletion)

            ).ConfigureAwait(false);
        }

        return entity;

        static async Task<string> ReadBlobAsync(BlobContainerClient containerClient, string auditPath)
        {
            var blobClient = containerClient
                .GetBlobClient(auditPath.Replace("//", $"/{Guid.Empty}/", StringComparison.OrdinalIgnoreCase));

            try
            {
                var blobResponse = await blobClient
                    .DownloadContentAsync()
                    .ConfigureAwait(false);

                return blobResponse.Value.Content.ToString();
            }
            catch (RequestFailedException exc) when (exc.ErrorCode == BlobErrorCode.ContainerNotFound)
            {
                return null;
            }
        }
    }

    public async IAsyncEnumerable<CommandAuditEntity> ListAsync(Guid organizationId, Guid? projectId = null, TimeSpan? timeRange = null, string[] commands = default)
    {
        var tableClient = await tableClientInstance
            .EnsureTableAsync()
            .ConfigureAwait(false);

        string filter;

        filter = TableClient.CreateQueryFilter<CommandAuditEntity>(e =>
            e.PartitionKey == organizationId.ToString()
        );

        if (projectId.HasValue)
        {
            filter += " and ";
            filter = TableClient.CreateQueryFilter<CommandAuditEntity>(e =>
                e.PartitionKey == organizationId.ToString()
            );
        }

        if (timeRange.HasValue)
        {
            filter += " and ";
            filter += TableClient.CreateQueryFilter<CommandAuditEntity>(e =>
                e.Timestamp >= DateTime.UtcNow.Subtract(timeRange.Value)
            );
        }

        if (commands?.Any() ?? false)
        {
            var commandConditions = commands
                .Select(cmd => cmd.EndsWith("<>", StringComparison.OrdinalIgnoreCase)
                ? GenerateFilterConditionStartsWith(nameof(CommandAuditEntity.Command), cmd.TrimEnd('>'))
                : TableClient.CreateQueryFilter<CommandAuditEntity>(e => e.Command == cmd.ToString()));

            filter += " and ";
            filter += $"({string.Join(") or (", commandConditions)})";
        }

        var entities = tableClient
            .QueryAsync<CommandAuditEntity>(filter: filter)
            .ConfigureAwait(false);

        await foreach (var entity in entities)
            yield return entity;
    }

    private static string GenerateFilterConditionStartsWith(string propertyName, string propertyValue)
    {
        var upperBound = new string(propertyValue.ToString()
            .ToCharArray().Reverse()
            .Select((c, i) => (char)(i == 0 ? c + 1 : c))
            .Reverse().ToArray());

        return $"{propertyName} ge '{propertyValue}' and {propertyName} lt '{upperBound}'";
    }
}
