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

namespace TeamCloud.Orchestrator.Command.Activities;

public sealed class CommandStatusActivity
{
    [FunctionName(nameof(CommandStatusActivity))]
    public Task<DurableOrchestrationStatus> RunActivity(
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

            return orchestrationClient
                .GetStatusAsync(input.CommandId.ToString(), showHistory: input.ShowHistory, showHistoryOutput: input.ShowHistoryOutput, showInput: input.ShowInput);
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

        public bool ShowHistory { get; set; }

        public bool ShowHistoryOutput { get; set; }

        public bool ShowInput { get; set; }
    }
}
