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
using TeamCloud.Azure.Storage;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Services;

public class OneTimeTokenService
{
    const string TableName = nameof(OneTimeTokenService);

    private readonly ITableService tableService;
    private readonly IOneTimeTokenServiceOptions options;

    public OneTimeTokenService(ITableService tableService, IOneTimeTokenServiceOptions options)
    {
        this.tableService = tableService ?? throw new ArgumentNullException(nameof(tableService));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<string> AcquireTokenAsync(User user, TimeSpan? ttl = null)
    {
        var client = await tableService
            .GetTableClientAsync(options.ConnectionString, TableName)
            .ConfigureAwait(false);

        var entity = new OneTimeTokenServiceEntity(Guid.Parse(user.Organization), Guid.Parse(user.Id), ttl);

        _ = await client
            .UpsertEntityAsync(entity)
            .ConfigureAwait(false);

        return entity.Token;
    }

    public async Task<OneTimeTokenServiceEntity> InvalidateTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException($"'{nameof(token)}' cannot be null or whitespace.", nameof(token));

        var timestamp = DateTimeOffset.UtcNow;

        var client = await tableService
            .GetTableClientAsync(options.ConnectionString, TableName)
            .ConfigureAwait(false);

        var filters = OneTimeTokenServiceEntity.DefaultPartitionKeyFilter;

        filters += $" and (RowKey eq {token} or Expires le {timestamp})";

        var entities = await client
            .QueryAsync<OneTimeTokenServiceEntity>(e =>
                e.PartitionKey == OneTimeTokenServiceEntity.DefaultPartitionKeyValue && (e.RowKey == token || e.Expires <= timestamp)
            ).ToListAsync()
            .ConfigureAwait(false);

        var entity = entities.SingleOrDefault(r => r.Token.Equals(token) && r.Expires > timestamp);

        var batch = new List<TableTransactionAction>();

        entities
            .ForEach(r => { r.ETag = ETag.All; batch.Add(new TableTransactionAction(TableTransactionActionType.Delete, r)); });

        if (batch.Any())
        {
            await client
                .SubmitTransactionAsync(batch)
                .ConfigureAwait(false);
        }

        return entity;
    }
}
