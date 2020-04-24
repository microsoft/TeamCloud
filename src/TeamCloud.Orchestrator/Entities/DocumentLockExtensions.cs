/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Entities
{
    public static class DocumentLockExtensions
    {

        internal static Task<IDisposable> LockAsync<TContainerDocument>(this IDurableOrchestrationContext functionContext, string containerDocumentId)
            where TContainerDocument : class, IContainerDocument
            => functionContext.LockAsync(functionContext.GetEntityId<TContainerDocument>(containerDocumentId));


        internal static Task<IDisposable> LockAsync<TContainerDocument>(this IDurableOrchestrationContext functionContext, TContainerDocument containerDocument)
            where TContainerDocument : class, IContainerDocument
            => functionContext.LockAsync(functionContext.GetEntityId<TContainerDocument>(containerDocument?.Id));


        internal static bool IsLockedBy<TContainerDocument>(this IDurableOrchestrationContext functionContext, string containerDocumentId)
            where TContainerDocument : class, IContainerDocument
            => functionContext.IsLocked(out var locks) && locks.Contains(functionContext.GetEntityId<TContainerDocument>(containerDocumentId));


        internal static bool IsLockedBy<TContainerDocument>(this IDurableOrchestrationContext functionContext, TContainerDocument containerDocument)
            where TContainerDocument : class, IContainerDocument
            => functionContext.IsLocked(out var locks) && locks.Contains(functionContext.GetEntityId<TContainerDocument>(containerDocument?.Id));


        private static EntityId GetEntityId<TContainerDocument>(this IDurableOrchestrationContext functionContext, string containerDocumentId)
            where TContainerDocument : class, IContainerDocument
        {
            if (string.IsNullOrWhiteSpace(containerDocumentId))
                throw new ArgumentException("A container document id must not NULL, EMPTY, or WHITESPACE", nameof(containerDocumentId));

            if (Guid.TryParse(containerDocumentId, out var containerDocumentGuid) && containerDocumentGuid == Guid.Empty)
                throw new ArgumentException("A container document id must not an empty GUID", nameof(containerDocumentId));

            return new EntityId(nameof(DocumentLockEntity), $"{containerDocumentId}@{typeof(TContainerDocument)}");
        }
    }
}
