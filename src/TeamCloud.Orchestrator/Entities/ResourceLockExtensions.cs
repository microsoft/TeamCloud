/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure.Resources;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Orchestrator.Entities
{
    public static class ResourceLockExtensions
    {
        private static EntityId GetEntityId(this IDurableOrchestrationContext orchestrationContext, Type entityType, string identifier)
        {
            if (entityType is null)
                throw new ArgumentNullException(nameof(entityType));

            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException("Identifier must not NULL, EMPTY, or WHITESPACE", nameof(identifier));

            return new EntityId(nameof(ResourceLockEntity), $"{entityType}|{identifier}");
        }

        internal static bool IsLockedBy<T>(this IDurableOrchestrationContext orchestrationContext, string identifier)
            where T : class
            => orchestrationContext.IsLocked(out var locks) && locks.Contains(orchestrationContext.GetEntityId(typeof(T), identifier));

        internal static Task<IDisposable> LockAsync<T>(this IDurableOrchestrationContext orchestrationContext, string identifier)
            where T : class
            => orchestrationContext.LockAsync(orchestrationContext.GetEntityId(typeof(T), identifier));

        internal static bool IsLockedByContainerDocument(this IDurableOrchestrationContext orchestrationContext, IContainerDocument containerDocument)
            => orchestrationContext.IsLocked(out var locks) && locks.Contains(orchestrationContext.GetEntityId(containerDocument?.GetType(), containerDocument?.Id));

        internal static Task<IDisposable> LockContainerDocumentAsync(this IDurableOrchestrationContext orchestrationContext, params IContainerDocument[] containerDocuments)
            => orchestrationContext.LockAsync(containerDocuments.Select(containerDocument => orchestrationContext.GetEntityId(containerDocument?.GetType(), containerDocument?.Id)).ToArray());

        internal static bool IsLockedByAzureResource(this IDurableOrchestrationContext orchestrationContext, AzureResourceIdentifier resourceId)
            => orchestrationContext.IsLocked(out var locks) && locks.Contains(orchestrationContext.GetEntityId(typeof(AzureResourceIdentifier), resourceId?.ToString()));

        internal static Task<IDisposable> LockAzureResourceAsync(this IDurableOrchestrationContext orchestrationContext, AzureResourceIdentifier resourceId)
            => orchestrationContext.LockAsync(orchestrationContext.GetEntityId(typeof(AzureResourceIdentifier), resourceId?.ToString()));


    }
}
