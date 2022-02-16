/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Command.Activities;

public sealed class CommandTerminateActivity
{
    [FunctionName(nameof(CommandTerminateActivity))]
    public async Task RunActivity(
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
            var input = activityContext.GetInput<Input>();

            await orchestrationClient
                .TerminateAsync(input.CommandId.ToString(), input.Reason ?? string.Empty)
                .ConfigureAwait(false);
        }
        catch (Exception exc)
        {
            log.LogError(exc, $"Failed to enqeueu command: {exc.Message}");

            throw exc.AsSerializable();
        }
    }

    internal struct Input
    {
        public Guid CommandId { get; set; }

        public string Reason { get; set; }
    }
}
