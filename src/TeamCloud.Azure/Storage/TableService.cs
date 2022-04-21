/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;

namespace TeamCloud.Azure.Storage;

public interface ITableService
{
    Task<TableClient> GetTableClientAsync(string connectionString, string tableName, bool ensureTable = true, CancellationToken cancellationToken = default);
}

public class TableService : ITableService
{
    private readonly ConcurrentDictionary<string, TableClient> tableClientMap = new(StringComparer.OrdinalIgnoreCase);

    public async Task<TableClient> GetTableClientAsync(string connectionString, string tableName, bool ensureTable = true, CancellationToken cancellationToken = default)
    {
        var accountName = connectionString
            .Split(';')
            .FirstOrDefault(p => p.StartsWith("AccountName=", StringComparison.OrdinalIgnoreCase))?
            .Split('=')
            .LastOrDefault();

        var key = $"{accountName ?? connectionString}{tableName.ToLowerInvariant()}";

        if (!tableClientMap.TryGetValue(key, out var tableClient))
        {
            tableClient = new TableClient(connectionString, tableName);

            if (ensureTable)
                await tableClient.CreateIfNotExistsAsync(cancellationToken)
                    .ConfigureAwait(false);

            tableClientMap[key] = tableClient;
        }

        return tableClient;
    }
}