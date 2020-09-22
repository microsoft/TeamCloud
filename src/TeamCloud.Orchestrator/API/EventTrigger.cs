/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure;
using TeamCloud.Data;
using TeamCloud.Http;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Orchestrations.Utilities;

namespace TeamCloud.Orchestrator.API
{
    public sealed class EventTrigger
    {
        private readonly IAzureSessionService azureSessionService;
        private readonly IProviderRepository providerRepository;
        private readonly IMemoryCache memoryCache;

        internal static async Task<string> GetUrlAsync()
        {
            var masterKey = await FunctionsEnvironment
                .GetAdminKeyAsync()
                .ConfigureAwait(false);

            var hostUrl = await FunctionsEnvironment
                .GetHostUrlAsync()
                .ConfigureAwait(false);

            var json = await hostUrl
                .AppendPathSegment("admin/host/systemkeys/eventgrid_extension")
                .SetQueryParam("code", masterKey, isEncoded: true)
                .GetJObjectAsync()
                .ConfigureAwait(false);

            return hostUrl
              .AppendPathSegment("runtime/webhooks/eventgrid")
              .SetQueryParam("functionName", nameof(EventTrigger))
              .SetQueryParam("code", json.SelectToken("value")?.ToString())
              .ToString();
        }

        public EventTrigger(IAzureSessionService azureSessionService, IProviderRepository providerRepository, IMemoryCache memoryCache)
        {
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
            this.providerRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
            this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }


        private async Task<IEnumerable<ProviderDocument>> GetProviderDocumentsAsync(EventGridEvent eventGridEvent)
        {
            static DateTime GetExpirationTimestamp()
            {
                var now = DateTime.UtcNow;
                var exp = now.AddMinutes(5 - now.Minute % 5);

                return exp.AddTicks(-(exp.Ticks % TimeSpan.TicksPerMinute));
            }

            bool DoesEventSubscriptionMatch(ProviderEventSubscription eventSubscription)
            {
                if (ProviderEventSubscription.All.Equals(eventSubscription))
                    return true;

                return eventGridEvent.EventType
                    .Equals(eventSubscription.EventType, StringComparison.Ordinal);
            }

            var providerDocuments = await memoryCache.GetOrCreateAsync<IEnumerable<ProviderDocument>>($"{this.GetType()}|{nameof(GetProviderDocumentsAsync)}", async cacheEntry =>
            {
                cacheEntry.SetAbsoluteExpiration(GetExpirationTimestamp());

                return await providerRepository
                    .ListAsync()
                    .Where(provider => provider.EventSubscriptions.Any())
                    .ToArrayAsync()
                    .ConfigureAwait(false);

            }).ConfigureAwait(false);

            return providerDocuments
                .Where(providerDocument => providerDocument.EventSubscriptions.Any(eventSubscription => DoesEventSubscriptionMatch(eventSubscription)));
        }

        [FunctionName(nameof(EventTrigger))]
        public async Task Run(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            [DurableClient] IDurableClient durableClient,
            ILogger log)
        {
            if (eventGridEvent is null)
                throw new ArgumentNullException(nameof(eventGridEvent));

            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            var providerDocuments = await GetProviderDocumentsAsync(eventGridEvent)
                .ConfigureAwait(false);

            if (providerDocuments.Any())
            {
                var systemIdentity = await azureSessionService
                    .GetIdentityAsync()
                    .ConfigureAwait(false);

                var systemUser = new User()
                {
                    Id = systemIdentity.ObjectId.ToString(),
                    Role = TeamCloudUserRole.None,
                    UserType = UserType.System
                };

                var command = new ProviderEventCommand(systemUser, eventGridEvent);

                var tasks = providerDocuments.Select(async providerDocument =>
                {
                    try
                    {
                        _ = await durableClient
                            .SendProviderCommandAsync(command, providerDocument)
                            .ConfigureAwait(false);

                        log.LogInformation($"Forwarded event {eventGridEvent.EventType} ({eventGridEvent.Id}) to provider {providerDocument.Id}");
                    }
                    catch (NotSupportedException exc)
                    {
                        log.LogWarning(exc, $"Forwarding event {eventGridEvent.EventType} ({eventGridEvent.Id}) to provider {providerDocument.Id} skipped: {exc.Message}");
                    }
                    catch (Exception exc)
                    {
                        log.LogError(exc, $"Forwarding event {eventGridEvent.EventType} ({eventGridEvent.Id}) to provider {providerDocument.Id} failed: {exc.Message}");

                        throw;
                    }
                });

                await Task
                    .WhenAll(tasks)
                    .ConfigureAwait(false);
            }
            else
            {
                log.LogDebug($"Ignoring event {eventGridEvent.EventType} ({eventGridEvent.Id})");
            }
        }
    }
}
