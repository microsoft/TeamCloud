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
        private static EntityId GetEntityId<T>(this IDurableOrchestrationContext functionContext, string identifier)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException("Identifier must not NULL, EMPTY, or WHITESPACE", nameof(identifier));

            return new EntityId(nameof(ResourceLockEntity), $"{typeof(T)}|{identifier}");
        }

        internal static bool IsLockedBy<T>(this IDurableOrchestrationContext functionContext, string identifier)
            where T : class
            => functionContext.IsLocked(out var locks) && locks.Contains(functionContext.GetEntityId<T>(identifier));

        internal static Task<IDisposable> LockAsync<T>(this IDurableOrchestrationContext functionContext, string identifier)
            where T : class
            => functionContext.LockAsync(functionContext.GetEntityId<T>(identifier));

        internal static bool IsLockedByContainerDocument<T>(this IDurableOrchestrationContext functionContext, T containerDocument)
            where T : class, IContainerDocument
            => functionContext.IsLocked(out var locks) && locks.Contains(functionContext.GetEntityId<T>(containerDocument?.Id));

        internal static Task<IDisposable> LockContainerDocumentAsync<T>(this IDurableOrchestrationContext functionContext, T containerDocument)
            where T : class, IContainerDocument
            => functionContext.LockAsync(functionContext.GetEntityId<T>(containerDocument?.Id));

        internal static bool IsLockedByAzureResource(this IDurableOrchestrationContext functionContext, AzureResourceIdentifier resourceId)
            => functionContext.IsLocked(out var locks) && locks.Contains(functionContext.GetEntityId<AzureResourceIdentifier>(resourceId?.ToString()));

        internal static Task<IDisposable> LockAzureResourceAsync(this IDurableOrchestrationContext functionContext, AzureResourceIdentifier resourceId)
            => functionContext.LockAsync(functionContext.GetEntityId<AzureResourceIdentifier>(resourceId?.ToString()));


    }
}
