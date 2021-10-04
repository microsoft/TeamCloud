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
using Microsoft.Extensions.Caching.Memory;
using TeamCloud.Data.CosmosDb.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

using DayOfWeek = TeamCloud.Model.Data.DayOfWeek;

namespace TeamCloud.Data.CosmosDb
{
    public sealed class CosmosDbScheduleRepository : CosmosDbRepository<Schedule>, IScheduleRepository
    {
        public CosmosDbScheduleRepository(ICosmosDbOptions options, IMemoryCache cache, IDocumentExpanderProvider expanderProvider = null, IDocumentSubscriptionProvider subscriptionProvider = null, IDataProtectionProvider dataProtectionProvider = null)
            : base(options, expanderProvider, subscriptionProvider, dataProtectionProvider, cache)
        { }

        public override async Task<Schedule> AddAsync(Schedule schedule)
        {
            if (schedule is null)
                throw new ArgumentNullException(nameof(schedule));

            await schedule
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .CreateItemAsync(schedule, GetPartitionKey(schedule))
                .ConfigureAwait(false);

            return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Create)
                .ConfigureAwait(false);
        }

        public override Task<Schedule> GetAsync(string projectId, string id, bool expand = false) => GetCachedAsync(projectId, id, async cached =>
        {
            if (projectId is null)
                throw new ArgumentNullException(nameof(projectId));

            if (id is null)
                throw new ArgumentNullException(nameof(id));

            if (!Guid.TryParse(id, out var idParsed))
                throw new ArgumentException("Value is not a valid GUID", nameof(id));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            Schedule schedule = null;

            try
            {
                var response = await container
                    .ReadItemAsync<Schedule>(idParsed.ToString(), GetPartitionKey(projectId))
                    .ConfigureAwait(false);

                schedule = SetCached(projectId, id, response.Resource);
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                schedule = cached;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            return await ExpandAsync(schedule, expand)
                .ConfigureAwait(false);
        });

        public override async IAsyncEnumerable<Schedule> ListAsync(string projectId)
        {
            if (projectId is null)
                throw new ArgumentNullException(nameof(projectId));

            if (!Guid.TryParse(projectId, out var projectIdParsed))
                throw new ArgumentException("Value is not a valid GUID", nameof(projectId));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition("SELECT * FROM c WHERE c.projectId = @projectId")
                .WithParameter("@projectId", projectIdParsed.ToString());

            var queryIterator = container
                .GetItemQueryIterator<Schedule>(query, requestOptions: GetQueryRequestOptions(projectId));

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                    yield return await ExpandAsync(queryResult).ConfigureAwait(false);
            }
        }

        public async IAsyncEnumerable<Schedule> ListAsync(string projectId, DayOfWeek day, int hour, int minute, int interval = 0)
        {
            if (projectId is null)
                throw new ArgumentNullException(nameof(projectId));

            if (!Guid.TryParse(projectId, out var projectIdParsed))
                throw new ArgumentException("Value is not a valid GUID", nameof(projectId));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var dayNames = Enum.GetNames(typeof(DayOfWeek));
            var dayIndex = (int)day;
            var dayName = dayNames[dayIndex];

            var wrap = interval > 0 && minute + interval > 59;
            var wrapDay = wrap && hour == 23 ? dayIndex == 6 ? dayNames[0] : dayNames[dayIndex + 1] : null;

            var dayQuery = string.IsNullOrEmpty(wrapDay) ? $"ARRAY_CONTAINS(c.daysOfWeek, '{dayName}')" : $"(ARRAY_CONTAINS(c.daysOfWeek, '{dayName}') OR ARRAY_CONTAINS(c.daysOfWeek, '{wrapDay}'))";
            var hourQuery = wrap ? hour == 23 ? $"(c.utcHour = {hour} OR c.utcHour = 0)" : $"(c.utcHour = {hour} OR c.utcHour = {hour + 1})" : $"c.utcHour = {hour}";
            var minuteQuery = interval <= 0 ? $"c.utcMinute = {minute}" : !wrap ? $"(c.utcMinute >= {minute} AND c.utcMinute < {minute + interval})" : $"(c.utcMinute >= {minute} OR c.utcMinute < {(minute + interval) % 59})";

            var queryString = $"SELECT * FROM c WHERE c.projectId = '{projectIdParsed}' AND {dayQuery} AND {hourQuery} AND {minuteQuery}";

            var query = new QueryDefinition(queryString);

            var schedules = container
                .GetItemQueryIterator<Schedule>(query, requestOptions: GetQueryRequestOptions(projectId))
                .ReadAllAsync(item => ExpandAsync(item))
                .ConfigureAwait(false);

            await foreach (var schedule in schedules)
                yield return schedule;
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
                    _ = await NotifySubscribersAsync(batchResponse.GetOperationResultResources<Schedule>(), DocumentSubscriptionEvent.Delete).ConfigureAwait(false);
                }
                else
                {
                    throw new Exception(batchResponse.ErrorMessage);
                }
            }
        }

        public override async Task<Schedule> RemoveAsync(Schedule schedule)
        {
            if (schedule is null)
                throw new ArgumentNullException(nameof(schedule));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .DeleteItemAsync<Schedule>(schedule.Id, GetPartitionKey(schedule))
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

        public override async Task<Schedule> SetAsync(Schedule schedule)
        {
            if (schedule is null)
                throw new ArgumentNullException(nameof(schedule));

            await schedule
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync(schedule, GetPartitionKey(schedule))
                .ConfigureAwait(false);

            return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Update)
                .ConfigureAwait(false);
        }
    }
}
