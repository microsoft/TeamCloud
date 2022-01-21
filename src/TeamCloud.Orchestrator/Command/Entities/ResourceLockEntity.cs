/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace TeamCloud.Orchestrator.Command.Entities;

public static class ResourceLockEntity
{
    [FunctionName(nameof(ResourceLockEntity))]
    public static void Run(
        [EntityTrigger] IDurableEntityContext entityContext,
        ILogger log)
    {
        // this entity represents a lock instance for critical sections
        // related to model classes implementing IContainerDocument.
        // to create a lock for a critical section inside an
        // orchestration  use the GetEntityId extension method
        // on the IContainerDocument interface

        if (entityContext is null)
            throw new ArgumentNullException(nameof(entityContext));

        if (log is null)
            throw new ArgumentNullException(nameof(log));

        log.LogInformation($"Lock acquired for document {entityContext.EntityKey}");
    }
}
