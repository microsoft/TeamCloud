/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Azure.Cosmos;
using TeamCloud.Data.CosmosDb.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

using DayOfWeek = TeamCloud.Model.Data.DayOfWeek;

namespace TeamCloud.Data.CosmosDb
{
    public sealed class CosmosDbScheduledTaskRepository : CosmosDbRepository<ScheduledTask>, IScheduledTaskRepository
    {
        public CosmosDbScheduledTaskRepository(ICosmosDbOptions options, IDocumentExpanderProvider expanderProvider = null, IDocumentSubscriptionProvider subscriptionProvider = null, IDataProtectionProvider dataProtectionProvider = null)
            : base(options, expanderProvider, subscriptionProvider, dataProtectionProvider)
        { }

        public override async Task<ScheduledTask> AddAsync(ScheduledTask task)
        {
            if (task is null)
                throw new ArgumentNullException(nameof(task));

            await task
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .CreateItemAsync(task, GetPartitionKey(task))
                .ConfigureAwait(false);

            return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Create)
                .ConfigureAwait(false);
        }

        public override async Task<ScheduledTask> GetAsync(string projectId, string id, bool expand = false)
        {
            if (projectId is null)
                throw new ArgumentNullException(nameof(projectId));

            if (id is null)
                throw new ArgumentNullException(nameof(id));

            if (!Guid.TryParse(id, out var idParsed))
                throw new ArgumentException("Value is not a valid GUID", nameof(id));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<ScheduledTask>(idParsed.ToString(), GetPartitionKey(projectId))
                    .ConfigureAwait(false);

                var expandTask = expand
                    ? ExpandAsync(response.Resource)
                    : Task.FromResult(response.Resource);

                return await expandTask.ConfigureAwait(false);
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public override async IAsyncEnumerable<ScheduledTask> ListAsync(string projectId)
        {
            if (projectId is null)
                throw new ArgumentNullException(nameof(projectId));

            if (!Guid.TryParse(projectId, out var projectIdParsed))
                throw new ArgumentException("Value is not a valid GUID", nameof(projectId));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var queryString = $"SELECT * FROM c WHERE c.projectId = '{projectIdParsed}'";

            var query = new QueryDefinition(queryString);

            var queryIterator = container
                .GetItemQueryIterator<ScheduledTask>(query, requestOptions: GetQueryRequestOptions(projectId));

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                    yield return queryResult;
            }
        }

        public async IAsyncEnumerable<ScheduledTask> ListAsync(string projectId, DayOfWeek day, int hour, int minute, int interval = 0)
        {
            if (projectId is null)
                throw new ArgumentNullException(nameof(projectId));

            if (!Guid.TryParse(projectId, out var projectIdParsed))
                throw new ArgumentException("Value is not a valid GUID", nameof(projectId));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var dayNames = Enum.GetNames(typeof(DayOfWeek)).Select(n => n.ToLowerInvariant()).ToArray();
            var dayIndex = (int)day;
            var dayName = dayNames[dayIndex];

            var wrap = interval > 0 && minute + interval > 59;
            var wrapDay = wrap && hour == 23 ? dayIndex == 6 ? dayNames[0] : dayNames[dayIndex + 1] : null;

            var dayQuery = string.IsNullOrEmpty(wrapDay) ? $"ARRAY_CONTAINS(c.dayOfWeek, '{dayName}')" : $"(ARRAY_CONTAINS(c.dayOfWeek, '{dayName}') OR ARRAY_CONTAINS(c.dayOfWeek, '{wrapDay}'))";
            var hourQuery = wrap ? hour == 23 ? $"(c.utcHour = {hour} OR c.utcHour = 0)" : $"(c.utcHour = {hour} OR c.utcHour = {hour + 1})" : $"c.utcHour = {hour}";
            var minuteQuery = interval <= 0 ? $"c.utcMinute = {minute}" : !wrap ? $"(c.utcMinute >= {minute} AND c.utcMinute < {minute + interval})" : $"(c.utcMinute >= {minute} OR c.utcMinute < {(minute + interval) % 59})";

            var queryString = $"SELECT * FROM c WHERE c.projectId = '{projectIdParsed}' AND {dayQuery} AND {hourQuery} AND {minuteQuery}";

            var query = new QueryDefinition(queryString);

            var queryIterator = container
                .GetItemQueryIterator<ScheduledTask>(query, requestOptions: GetQueryRequestOptions(projectId));

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                    yield return queryResult;
            }
        }

        public async Task RemoveAllAsync(string projectId)
        {
            var components = ListAsync(projectId);

            if (await components.AnyAsync().ConfigureAwait(false))
            {
                var container = await GetContainerAsync()
                    .ConfigureAwait(false);

                var batch = container
                    .CreateTransactionalBatch(GetPartitionKey(projectId));

                await foreach (var component in components.ConfigureAwait(false))
                    batch = batch.DeleteItem(component.Id);

                var batchResponse = await batch
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                if (batchResponse.IsSuccessStatusCode)
                {
                    _ = await NotifySubscribersAsync(batchResponse.GetOperationResultResources<ScheduledTask>(), DocumentSubscriptionEvent.Delete).ConfigureAwait(false);
                }
                else
                {
                    throw new Exception(batchResponse.ErrorMessage);
                }
            }
        }

        public override async Task<ScheduledTask> RemoveAsync(ScheduledTask task)
        {
            if (task is null)
                throw new ArgumentNullException(nameof(task));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .DeleteItemAsync<ScheduledTask>(task.Id, GetPartitionKey(task))
                    .ConfigureAwait(false);

                return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Delete)
                    .ConfigureAwait(false);
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null; // already deleted
            }
        }

        public async Task RemoveAsync(string projectId, string id)
        {
            var component = await GetAsync(projectId, id)
                .ConfigureAwait(false);

            if (component != null)
            {
                await RemoveAsync(component)
                    .ConfigureAwait(false);
            }
        }

        public override async Task<ScheduledTask> SetAsync(ScheduledTask task)
        {
            if (task is null)
                throw new ArgumentNullException(nameof(task));

            await task
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync(task, GetPartitionKey(task))
                .ConfigureAwait(false);

            return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Update)
                .ConfigureAwait(false);
        }
    }
}
