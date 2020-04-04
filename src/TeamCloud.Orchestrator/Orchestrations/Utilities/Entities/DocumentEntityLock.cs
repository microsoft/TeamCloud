/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities.Entities
{
    public static class DocumentEntityLock
    {
        [FunctionName(nameof(DocumentEntityLock))]
        public static void Run([EntityTrigger] IDurableEntityContext entityContext)
        {
            // this entity represents a lock instance for critical sections
            // related to model classes implementing IContainerDocument.
            // to create a lock for a critical section inside an
            // orchestration  use the GetEntityId extension method
            // on the IContainerDocument interface

            if (entityContext is null)
                throw new ArgumentNullException(nameof(entityContext));
        }
    }
}
