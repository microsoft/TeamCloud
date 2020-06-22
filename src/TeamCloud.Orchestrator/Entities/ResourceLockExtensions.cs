/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure.Resources;
using TeamCloud.Model.Internal.Data.Core;

namespace TeamCloud.Orchestrator.Entities
{
    public static class ResourceLockExtensions
    {
        private static EntityId GetEntityId(this IDurableOrchestrationContext functionContext, Type entityType, string identifier)
        {
            if (entityType is null)
                throw new ArgumentNullException(nameof(entityType));

            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException("Identifier must not NULL, EMPTY, or WHITESPACE", nameof(identifier));

            return new EntityId(nameof(ResourceLockEntity), $"{entityType}|{identifier}");
        }

        internal static bool IsLockedBy<T>(this IDurableOrchestrationContext functionContext, string identifier)
            where T : class
            => functionContext.IsLocked(out var locks) && locks.Contains(functionContext.GetEntityId(typeof(T), identifier));

        internal static Task<IDisposable> LockAsync<T>(this IDurableOrchestrationContext functionContext, string identifier)
            where T : class
            => functionContext.LockAsync(functionContext.GetEntityId(typeof(T), identifier));

        internal static bool IsLockedByContainerDocument(this IDurableOrchestrationContext functionContext, IContainerDocument containerDocument)
            => functionContext.IsLocked(out var locks) && locks.Contains(functionContext.GetEntityId(containerDocument?.GetType(), containerDocument?.Id));

        internal static Task<IDisposable> LockContainerDocumentAsync(this IDurableOrchestrationContext functionContext, params IContainerDocument[] containerDocuments)
            => functionContext.LockAsync(containerDocuments.Select(containerDocument => functionContext.GetEntityId(containerDocument?.GetType(), containerDocument?.Id)).ToArray());

        internal static bool IsLockedByAzureResource(this IDurableOrchestrationContext functionContext, AzureResourceIdentifier resourceId)
            => functionContext.IsLocked(out var locks) && locks.Contains(functionContext.GetEntityId(typeof(AzureResourceIdentifier), resourceId?.ToString()));

        internal static Task<IDisposable> LockAzureResourceAsync(this IDurableOrchestrationContext functionContext, AzureResourceIdentifier resourceId)
            => functionContext.LockAsync(functionContext.GetEntityId(typeof(AzureResourceIdentifier), resourceId?.ToString()));


    }
}
