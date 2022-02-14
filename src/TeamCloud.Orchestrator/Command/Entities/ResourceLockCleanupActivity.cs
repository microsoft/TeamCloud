/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using TeamCloud.Serialization;
using System.Threading;

namespace TeamCloud.Orchestrator.Command.Entities;

public static class ResourceLockCleanupActivity
{
    [FunctionName(nameof(ResourceLockCleanupActivity))]
    public static Task RunActivity(
        [ActivityTrigger] IDurableActivityContext activityContext,
        [DurableClient] IDurableClient orchestrationClient,
        ILogger log)
    {
        if (activityContext is null)
            throw new ArgumentNullException(nameof(activityContext));

        if (orchestrationClient is null)
            throw new ArgumentNullException(nameof(orchestrationClient));

        if (log is null)
            throw new ArgumentNullException(nameof(log));

        try
        {
            return orchestrationClient
                .CleanEntityStorageAsync(true, true, CancellationToken.None);
        }
        catch (Exception exc)
        {
            log.LogError(exc, $"Failed to enqeueu command: {exc.Message}");

            throw exc.AsSerializable();
        }
    }
}
