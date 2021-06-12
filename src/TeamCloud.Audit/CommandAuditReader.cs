using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Blob;
using TeamCloud.Audit.Model;
using BlobCloudStorageAccount = Microsoft.Azure.Storage.CloudStorageAccount;
using TableCloudStorageAccount = Microsoft.Azure.Cosmos.Table.CloudStorageAccount;

namespace TeamCloud.Audit
{
    public class CommandAuditReader : ICommandAuditReader
    {
        private readonly ICommandAuditOptions options;
        private readonly Lazy<CloudBlobContainer> auditContainerInstance;
        private readonly Lazy<CloudTable> auditTableInstance;

        public CommandAuditReader(ICommandAuditOptions options = null)
        {
            this.options = options ?? CommandAuditOptions.Default;

            auditContainerInstance = new Lazy<CloudBlobContainer>(() => BlobCloudStorageAccount
                .Parse(this.options.ConnectionString)
                .CreateCloudBlobClient().GetContainerReference(CommandAuditEntity.AUDIT_CONTAINER_NAME));

            auditTableInstance = new Lazy<CloudTable>(() => TableCloudStorageAccount
                .Parse(this.options.ConnectionString)
                .CreateCloudTableClient().GetTableReference(CommandAuditEntity.AUDIT_TABLE_NAME));
        }

        public async Task<CommandAuditEntity> GetAsync(Guid organizationId, Guid commandId, bool includeJsonDumps = false)
        {
            var auditTable = await auditTableInstance
                .EnsureTableAsync()
                .ConfigureAwait(false);

            try
            {
                var result = await auditTable
                    .ExecuteAsync(TableOperation.Retrieve<CommandAuditEntity>(organizationId.ToString(), commandId.ToString()))
                    .ConfigureAwait(false);

                var entity = result.Result as CommandAuditEntity;

                if (entity != null && includeJsonDumps) Task.WaitAll(

                    ReadBlobAsync(entity.GetCommandPath())
                        .ContinueWith(t => entity.CommandJson = t.Result, TaskContinuationOptions.OnlyOnRanToCompletion),

                    ReadBlobAsync(entity.GetResultPath())
                        .ContinueWith(t => entity.ResultJson = t.Result, TaskContinuationOptions.OnlyOnRanToCompletion)
                );

                return entity;
            }
            catch (StorageException exc) when (exc.RequestInformation?.HttpStatusCode == 404)
            {
                return null;
            }

            async Task<string> ReadBlobAsync(string auditPath)
            {
                var auditContainer = await auditContainerInstance
                    .EnsureContainerAsync()
                    .ConfigureAwait(false);

                var auditBlob = auditContainer.GetBlockBlobReference(auditPath.Replace("//", $"/{Guid.Empty}/", StringComparison.OrdinalIgnoreCase));

                try
                {
                    return await auditBlob
                        .DownloadTextAsync()
                        .ConfigureAwait(false);
                }
                catch (StorageException exc) when (exc.RequestInformation?.HttpStatusCode == 404)
                {
                    return null;
                }
            }
        }

        public async IAsyncEnumerable<CommandAuditEntity> ListAsync(Guid organizationId, Guid? projectId = null, TimeSpan? timeRange = null, string[]? commands = null)
        {
            var auditTable = await auditTableInstance
                .EnsureTableAsync()
                .ConfigureAwait(false);

            string filter;

            filter = TableQuery.GenerateFilterCondition(
                    AuditEntity.PartitionKeyName,
                    QueryComparisons.Equal,
                    organizationId.ToString());

            if (projectId.HasValue)
            {
                filter = TableQuery.CombineFilters(
                    filter,
                    TableOperators.And, 
                    TableQuery.GenerateFilterCondition(
                        nameof(CommandAuditEntity.ProjectId),
                        QueryComparisons.Equal,
                        projectId.ToString()));
            }

            if (timeRange.HasValue)
            {
                filter = TableQuery.CombineFilters(
                    filter,
                    TableOperators.And,
                    TableQuery.GenerateFilterConditionForDate(AuditEntity.TimestampName,
                        QueryComparisons.GreaterThanOrEqual,
                        DateTime.UtcNow.Subtract(timeRange.Value)));
            }

            if (commands?.Any() ?? false)
            {
                var commandConditions = commands
                    .Select(cmd => cmd.EndsWith("<>", StringComparison.OrdinalIgnoreCase)
                    ? GenerateFilterConditionStartsWith(nameof(CommandAuditEntity.Command), cmd.TrimEnd('>'))
                    : TableQuery.GenerateFilterCondition(nameof(CommandAuditEntity.Command), QueryComparisons.Equal, cmd.ToString()));

                filter = TableQuery.CombineFilters(
                    filter,
                    TableOperators.And,
                    $"({string.Join(") or (", commandConditions)})");
            }

            var query = new TableQuery<CommandAuditEntity>()
                .Where(filter)
                .OrderByDesc(nameof(CommandAuditEntity.Created));

            TableContinuationToken continuationToken = null;

            while (true)
            {
                var result = await auditTable
                    .ExecuteQuerySegmentedAsync(query, continuationToken)
                    .ConfigureAwait(false);

                foreach (var entity in result.Results)
                    yield return entity;

                continuationToken = result.ContinuationToken;

                if (continuationToken is null)
                    break;
            }
        }

        private static string GenerateFilterConditionStartsWith(string propertyName, string propertyValue)
        {
            var upperBound = new string(propertyValue.ToString()
                .ToCharArray().Reverse()
                .Select((c, i) => (char)(i == 0 ? c + 1 : c))
                .Reverse().ToArray());

            return TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition(propertyName,
                    QueryComparisons.GreaterThanOrEqual,
                    propertyValue),
                TableOperators.And,
                TableQuery.GenerateFilterCondition(propertyName,
                    QueryComparisons.LessThan,
                    upperBound)
                );
        }
    }
}
