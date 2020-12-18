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

namespace TeamCloud.Orchestrator.Operations.Entities
{
    public static class ResourceLockExtensions
    {
        private static EntityId GetEntityId(this IDurableOrchestrationContext orchestrationContext, Type entityType, string identifier, params string[] qualifiers)
        {
            if (entityType is null)
                throw new ArgumentNullException(nameof(entityType));

            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException("Identifier must not NULL, EMPTY, or WHITESPACE", nameof(identifier));

            return new EntityId(nameof(ResourceLockEntity), $"{entityType}|{identifier}|{string.Join('|', qualifiers.Where(q => !string.IsNullOrWhiteSpace(q)))}".TrimEnd('|'));
        }

        internal static bool IsLockedBy<T>(this IDurableOrchestrationContext orchestrationContext, string identifier, params string[] qualifiers)
            where T : class
            => orchestrationContext.IsLocked(out var locks) && locks.Contains(orchestrationContext.GetEntityId(typeof(T), identifier, qualifiers));

        internal static Task<IDisposable> LockAsync<T>(this IDurableOrchestrationContext orchestrationContext, string identifier, params string[] qualifiers)
            where T : class
            => orchestrationContext.LockAsync(orchestrationContext.GetEntityId(typeof(T), identifier, qualifiers));

        internal static bool IsLockedByContainerDocument(this IDurableOrchestrationContext orchestrationContext, IContainerDocument containerDocument, params string[] qualifiers)
            => orchestrationContext.IsLocked(out var locks) && locks.Contains(orchestrationContext.GetEntityId(containerDocument?.GetType(), containerDocument?.Id, qualifiers));

        internal static Task<IDisposable> LockContainerDocumentAsync(this IDurableOrchestrationContext orchestrationContext, IContainerDocument containerDocument, params string[] qualifiers)
            => orchestrationContext.LockAsync(orchestrationContext.GetEntityId(containerDocument?.GetType(), containerDocument?.Id, qualifiers));

        internal static bool IsLockedByAzureResource(this IDurableOrchestrationContext orchestrationContext, AzureResourceIdentifier resourceId, params string[] qualifiers)
            => orchestrationContext.IsLocked(out var locks) && locks.Contains(orchestrationContext.GetEntityId(typeof(AzureResourceIdentifier), resourceId?.ToString(), qualifiers));

        internal static Task<IDisposable> LockAzureResourceAsync(this IDurableOrchestrationContext orchestrationContext, AzureResourceIdentifier resourceId, params string[] qualifiers)
            => orchestrationContext.LockAsync(orchestrationContext.GetEntityId(typeof(AzureResourceIdentifier), resourceId?.ToString(), qualifiers));
    }
}
