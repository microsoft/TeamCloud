using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.Azure.Cosmos.Table;
using Nito.AsyncEx;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Services
{
    public class OneTimeTokenService
    {
        private readonly IOneTimeTokenServiceOptions options;
        private readonly AsyncLazy<CloudTable> tableInstance;

        public OneTimeTokenService(IOneTimeTokenServiceOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            tableInstance = new AsyncLazy<CloudTable>(async () =>
            {
                var table = CloudStorageAccount
                    .Parse(this.options.ConnectionString)
                    .CreateCloudTableClient()
                    .GetTableReference(nameof(OneTimeTokenService));

                await table
                    .CreateIfNotExistsAsync()
                    .ConfigureAwait(false);

                return table;
            });
        }

        public async Task<string> AcquireTokenAsync(User user, TimeSpan? ttl = null)
        {
            var table = await tableInstance.ConfigureAwait(false);
            var entity = new OneTimeTokenServiceEntity(Guid.Parse(user.Organization), Guid.Parse(user.Id), ttl);

            _ = await table
                .ExecuteAsync(TableOperation.Insert(entity))
                .ConfigureAwait(false);

            return entity.Token;
        }

        public async Task<OneTimeTokenServiceEntity> InvalidateTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException($"'{nameof(token)}' cannot be null or whitespace.", nameof(token));

            var timestamp = DateTimeOffset.UtcNow;

            var table = await tableInstance.ConfigureAwait(false);

            var filter = TableQuery.CombineFilters(
                OneTimeTokenServiceEntity.DefaultPartitionKeyFilter,
                TableOperators.And, 
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, token),
                    TableOperators.Or,
                    TableQuery.GenerateFilterConditionForDate("Expires", QueryComparisons.LessThanOrEqual, timestamp)));

            var response = await table
                .ExecuteQuerySegmentedAsync(new TableQuery<OneTimeTokenServiceEntity>().Where(filter), default)
                .ConfigureAwait(false);

            OneTimeTokenServiceEntity entity = null;

            while (true)
            {
                entity ??= response.Results
                    .SingleOrDefault(r => r.Token.Equals(token) && r.Expires > timestamp);

                var batch = new TableBatchOperation();

                response.Results
                    .ToList()
                    .ForEach(r => { r.TableEntity.ETag = "*"; batch.Add(TableOperation.Delete(r)); });

                if (batch.Any())
                {
                    await table
                        .ExecuteBatchAsync(batch)
                        .ConfigureAwait(false);
                }

                if (response.ContinuationToken != null)
                {
                    response = await table
                        .ExecuteQuerySegmentedAsync(new TableQuery<OneTimeTokenServiceEntity>().Where(filter), response.ContinuationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    break;
                }
            }

            return entity;
        }
    }
}
